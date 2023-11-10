using Common.Shared;
using Common.Shared.Auth;
using Microsoft.EntityFrameworkCore;
using ApiSession = Common.Shared.Auth.Session;

namespace Common.Server.Auth;

public class AuthEps<TDbCtx>
    where TDbCtx : DbContext, IAuthDb
{
    private readonly int _maxAuthAttemptsPerSecond;
    private readonly Func<IRpcCtx, TDbCtx, string, string, Task> _onActivation;
    private readonly Func<IRpcCtx, TDbCtx, Session, Task> _onDelete;
    private readonly Func<IRpcCtx, TDbCtx, Session, IReadOnlyList<string>, Task> _validateFcmTopic;

    public AuthEps(
        int maxAuthAttemptsPerSecond,
        Func<IRpcCtx, TDbCtx, string, string, Task> onActivation,
        Func<IRpcCtx, TDbCtx, Session, Task> onDelete,
        Func<IRpcCtx, TDbCtx, Session, IReadOnlyList<string>, Task> validateFcmTopic
    )
    {
        _maxAuthAttemptsPerSecond = maxAuthAttemptsPerSecond;
        _onActivation = onActivation;
        _onDelete = onDelete;
        _validateFcmTopic = validateFcmTopic;
        Eps = new List<IRpcEndpoint>
        {
            new RpcEndpoint<Nothing, ApiSession>(
                AuthRpcs.GetSession,
                (ctx, _) => ctx.GetSession().ToApi().AsTask()
            ),
            new RpcEndpoint<Register, Nothing>(
                AuthRpcs.Register,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
                    return await ctx.DbTx<TDbCtx, Nothing>(
                        async (db, ses) =>
                        {
                            var (auth, newCreated) = await CreateAuth(
                                ctx,
                                db,
                                req,
                                ses.Id,
                                ses.Lang,
                                ses.DateFmt,
                                ses.TimeFmt
                            );

                            if (
                                !auth.VerifyEmailCode.IsNullOrEmpty()
                                && (
                                    newCreated
                                    || (
                                        auth.VerifyEmailCodeCreatedOn.MinutesSince() > 10
                                        && auth.ActivatedOn.IsZero()
                                    )
                                )
                            )
                            {
                                // if there is a verify email code and
                                // we've just registered a new account
                                // or the verify email was sent over 10 mins ago
                                // and the account is not yet activated
                                var config = ctx.Get<IConfig>();
                                var emailClient = ctx.Get<IEmailClient>();
                                var model = new
                                {
                                    BaseHref = config.Server.Listen,
                                    auth.Email,
                                    Code = auth.VerifyEmailCode
                                };
                                await emailClient.SendEmailAsync(
                                    ctx.String(S.AuthConfirmEmailSubject),
                                    ctx.String(S.AuthConfirmEmailHtml, model),
                                    ctx.String(S.AuthConfirmEmailText, model),
                                    config.Email.NoReplyAddress,
                                    new List<string> { req.Email },
                                    null,
                                    null,
                                    ctx.Ctkn
                                );
                            }
                            return Nothing.Inst;
                        },
                        false
                    );
                }
            ),
            new RpcEndpoint<VerifyEmail, Nothing>(
                AuthRpcs.VerifyEmail,
                async (ctx, req) =>
                {
                    // !!! ToLower all emails in all Auth_ api endpoints
                    req = req with
                    {
                        Email = req.Email.ToLower()
                    };
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    return await ctx.DbTx<TDbCtx, Nothing>(
                        async (db, ses) =>
                        {
                            var auth = await db.Auths.SingleOrDefaultAsync(
                                x =>
                                    x.Email.Equals(req.Email)
                                    || (x.NewEmail != null && x.NewEmail.Equals(req.Email)),
                                ctx.Ctkn
                            );
                            ctx.NotFoundIf(auth == null, model: new { Name = "Auth" });
                            ctx.ErrorIf(
                                auth.NotNull().VerifyEmailCode != req.Code,
                                S.AuthInvalidEmailCode
                            );
                            if (!auth.NewEmail.IsNullOrEmpty() && auth.NewEmail == req.Email)
                            {
                                // verifying new email
                                auth.Email = auth.NewEmail;
                                auth.NewEmail = string.Empty;
                            }
                            else
                            {
                                // first account activation
                                auth.ActivatedOn = DateTimeExt.UtcNowMilli();
                                await _onActivation(ctx, db, auth.Id, auth.Email);
                            }

                            auth.VerifyEmailCodeCreatedOn = DateTimeExt.Zero();
                            auth.VerifyEmailCode = string.Empty;
                            return Nothing.Inst;
                        },
                        false
                    );
                }
            ),
            new RpcEndpoint<SendResetPwdEmail, Nothing>(
                AuthRpcs.SendResetPwdEmail,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
                    // !!! ToLower all emails in all Auth_ api endpoints
                    req = new SendResetPwdEmail(req.Email.ToLower());
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    return await ctx.DbTx<TDbCtx, Nothing>(
                        async (db, ses) =>
                        {
                            var existing = await db.Auths.SingleOrDefaultAsync(
                                x => x.Email.Equals(req.Email),
                                ctx.Ctkn
                            );
                            if (
                                existing == null
                                || existing.ResetPwdCodeCreatedOn.MinutesSince() < 10
                            )
                                // if email is not associated with an account or
                                // a reset pwd was sent within the last 10 minutes
                                // dont do anything
                                return Nothing.Inst;

                            existing.ResetPwdCodeCreatedOn = DateTimeExt.UtcNowMilli();
                            existing.ResetPwdCode = Crypto.String(32);
                            await db.SaveChangesAsync(ctx.Ctkn);
                            var config = ctx.Get<IConfig>();
                            var model = new
                            {
                                BaseHref = config.Server.Listen,
                                existing.Email,
                                Code = existing.ResetPwdCode
                            };
                            var emailClient = ctx.Get<IEmailClient>();
                            await emailClient.SendEmailAsync(
                                ctx.String(S.AuthResetPwdSubject),
                                ctx.String(S.AuthResetPwdHtml, model),
                                ctx.String(S.AuthResetPwdText, model),
                                config.Email.NoReplyAddress,
                                new List<string> { req.Email },
                                null,
                                null,
                                ctx.Ctkn
                            );
                            return Nothing.Inst;
                        },
                        false
                    );
                }
            ),
            new RpcEndpoint<ResetPwd, Nothing>(
                AuthRpcs.ResetPwd,
                async (ctx, req) =>
                {
                    // !!! ToLower all emails in all Auth_ api endpoints
                    req = req with
                    {
                        Email = req.Email.ToLower()
                    };
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    ctx.ErrorFromValidationResult(AuthValidator.Pwd(req.NewPwd));
                    return await ctx.DbTx<TDbCtx, Nothing>(
                        async (db, ses) =>
                        {
                            var auth = await db.Auths.FirstOrDefaultAsync(
                                x => x.Email.Equals(req.Email),
                                ctx.Ctkn
                            );
                            ctx.NotFoundIf(auth == null, model: new { Name = "Auth" });
                            ctx.ErrorIf(
                                auth.NotNull().ResetPwdCode != req.Code
                                    && auth.ResetPwdCodeCreatedOn.MinutesSince() > 10,
                                S.AuthInvalidResetPwdCode
                            );
                            var pwd = Crypto.HashPwd(req.NewPwd);
                            auth.ResetPwdCodeCreatedOn = DateTimeExt.Zero();
                            auth.ResetPwdCode = string.Empty;
                            auth.PwdVersion = pwd.PwdVersion;
                            auth.PwdSalt = pwd.PwdSalt;
                            auth.PwdHash = pwd.PwdHash;
                            auth.PwdIters = pwd.PwdIters;
                            return Nothing.Inst;
                        },
                        false
                    );
                }
            ),
            new RpcEndpoint<SendMagicLinkEmail, Nothing>(
                AuthRpcs.SendMagicLinkEmail,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
                    // !!! ToLower all emails in all Auth api endpoints
                    req = req with
                    {
                        Email = req.Email.ToLower()
                    };
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    return await ctx.DbTx<TDbCtx, Nothing>(
                        async (db, _) =>
                        {
                            var auth = await db.Auths.SingleOrDefaultAsync(
                                x => x.Email.Equals(req.Email),
                                ctx.Ctkn
                            );
                            if (auth == null || auth.MagicLinkCodeCreatedOn.MinutesSince() < 10)
                                // if email is not associated with an account or
                                // a reset pwd was sent within the last 10 minutes
                                // dont do anything
                                return Nothing.Inst;

                            auth.MagicLinkCodeCreatedOn = DateTimeExt.UtcNowMilli();
                            auth.MagicLinkCode = Crypto.String(32);
                            await db.SaveChangesAsync(ctx.Ctkn);
                            var config = ctx.Get<IConfig>();
                            var model = new
                            {
                                BaseHref = config.Server.Listen,
                                auth.Email,
                                Code = auth.MagicLinkCode,
                                req.RememberMe
                            };
                            var emailClient = ctx.Get<IEmailClient>();
                            await emailClient.SendEmailAsync(
                                ctx.String(S.AuthMagicLinkSubject),
                                ctx.String(S.AuthMagicLinkHtml, model),
                                ctx.String(S.AuthMagicLinkText, model),
                                config.Email.NoReplyAddress,
                                new List<string> { req.Email },
                                null,
                                null,
                                ctx.Ctkn
                            );
                            return Nothing.Inst;
                        },
                        false
                    );
                }
            ),
            new RpcEndpoint<MagicLinkSignIn, ApiSession>(
                AuthRpcs.MagicLinkSignIn,
                async (ctx, req) =>
                {
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
                    // !!! ToLower all emails in all Auth api endpoints
                    req = req with
                    {
                        Email = req.Email.ToLower()
                    };
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    return await ctx.DbTx<TDbCtx, ApiSession>(
                        async (db, ses) =>
                        {
                            var auth = await db.Auths.SingleOrDefaultAsync(
                                x => x.Email.Equals(req.Email),
                                ctx.Ctkn
                            );
                            ctx.NotFoundIf(auth == null, model: new { Name = "Auth" });
                            ctx.ErrorIf(
                                auth.NotNull().ActivatedOn.IsZero(),
                                S.AuthAccountNotVerified
                            );
                            RateLimitAuthAttempts(ctx, auth.NotNull());
                            auth.LastSignInAttemptOn = DateTimeExt.UtcNowMilli();
                            ctx.NotFoundIf(
                                auth.MagicLinkCode != req.Code
                                    || auth.MagicLinkCodeCreatedOn.MinutesSince() > 10,
                                model: new { Name = "Auth" }
                            );
                            auth.MagicLinkCode = string.Empty;
                            auth.MagicLinkCodeCreatedOn = DateTimeExt.Zero();
                            auth.LastSignedInOn = DateTimeExt.UtcNowMilli();
                            return ctx.CreateSession(
                                    auth.Id,
                                    true,
                                    req.RememberMe,
                                    auth.Lang,
                                    auth.DateFmt,
                                    auth.TimeFmt,
                                    auth.FcmEnabled
                                )
                                .ToApi();
                        },
                        false
                    );
                }
            ),
            new RpcEndpoint<SignIn, ApiSession>(
                AuthRpcs.SignIn,
                async (ctx, req) =>
                {
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
                    // !!! ToLower all emails in all Auth_ api endpoints
                    req = req with
                    {
                        Email = req.Email.ToLower()
                    };
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    return await ctx.DbTx<TDbCtx, ApiSession>(
                        async (db, ses) =>
                        {
                            var auth = await db.Auths.SingleOrDefaultAsync(
                                x => x.Email.Equals(req.Email),
                                ctx.Ctkn
                            );
                            ctx.NotFoundIf(auth == null, model: new { Name = "Auth" });
                            ctx.ErrorIf(
                                auth.NotNull().ActivatedOn.IsZero(),
                                S.AuthAccountNotVerified
                            );
                            RateLimitAuthAttempts(ctx, auth.NotNull());
                            auth.LastSignInAttemptOn = DateTimeExt.UtcNowMilli();
                            ctx.NotFoundIf(
                                !Crypto.PwdIsValid(req.Pwd, auth),
                                model: new { Name = "Auth" }
                            );
                            auth.LastSignedInOn = DateTimeExt.UtcNowMilli();
                            return ctx.CreateSession(
                                    auth.Id,
                                    true,
                                    req.RememberMe,
                                    auth.Lang,
                                    auth.DateFmt,
                                    auth.TimeFmt,
                                    auth.FcmEnabled
                                )
                                .ToApi();
                        },
                        false
                    );
                }
            ),
            new RpcEndpoint<Nothing, ApiSession>(
                AuthRpcs.SignOut,
                async (ctx, _) =>
                {
                    var ses = ctx.GetSession();
                    if (ses.IsAuthed)
                    {
                        var db = ctx.Get<TDbCtx>();
                        var fcm = ctx.Get<IFcmClient>();
                        var tokens = await db.FcmRegs
                            .Where(x => x.User == ses.Id)
                            .Select(x => x.Token)
                            .Distinct()
                            .ToListAsync(ctx.Ctkn);
                        await fcm.SendRaw(ctx, FcmType.SignOut, tokens, "", null);
                        ses = ctx.ClearSession();
                    }

                    return ses.ToApi();
                }
            ),
            new RpcEndpoint<Nothing, ApiSession>(
                AuthRpcs.Delete,
                async (ctx, _) =>
                    await ctx.DbTx<TDbCtx, ApiSession>(
                        async (db, ses) =>
                        {
                            await _onDelete(ctx, db, ses);
                            await db.Auths.Where(x => x.Id == ses.Id).ExecuteDeleteAsync(ctx.Ctkn);
                            await db.FcmRegs
                                .Where(x => x.User == ses.Id)
                                .ExecuteDeleteAsync(ctx.Ctkn);
                            ses = ctx.ClearSession();
                            return ses.ToApi();
                        }
                    )
            ),
            new RpcEndpoint<SetL10n, ApiSession>(
                AuthRpcs.SetL10n,
                async (ctx, req) =>
                {
                    var ses = ctx.GetSession();
                    if (
                        (req.Lang.IsNullOrWhiteSpace() || ses.Lang == req.Lang)
                        && (req.DateFmt.IsNullOrWhiteSpace() || ses.DateFmt == req.DateFmt)
                        && (req.TimeFmt.IsNullOrWhiteSpace() || ses.TimeFmt == req.TimeFmt)
                    )
                    {
                        return ses.ToApi();
                    }

                    var s = ctx.Get<S>();
                    ses = ctx.CreateSession(
                        ses.Id,
                        ses.IsAuthed,
                        ses.RememberMe,
                        s.BestLang(req.Lang),
                        req.DateFmt,
                        req.TimeFmt,
                        ses.FcmEnabled
                    );
                    if (ses.IsAuthed)
                    {
                        await ctx.DbTx<TDbCtx, Nothing>(
                            async (db, _) =>
                            {
                                var auth = await db.Auths.SingleOrDefaultAsync(
                                    x => x.Id == ses.Id,
                                    ctx.Ctkn
                                );
                                ctx.NotFoundIf(auth == null, model: new { Name = "Auth" });
                                ctx.ErrorIf(
                                    auth.NotNull().ActivatedOn.IsZero(),
                                    S.AuthAccountNotVerified
                                );
                                auth.Lang = ses.Lang;
                                auth.DateFmt = ses.DateFmt;
                                auth.TimeFmt = ses.TimeFmt;
                                return Nothing.Inst;
                            }
                        );
                    }

                    return ses.ToApi();
                }
            ),
            new RpcEndpoint<FcmEnabled, ApiSession>(
                AuthRpcs.FcmEnabled,
                async (ctx, req) =>
                    await ctx.DbTx<TDbCtx, ApiSession>(
                        async (db, ses) =>
                        {
                            if (ses.FcmEnabled == req.Val)
                            {
                                // not changing anything
                                return ses.ToApi();
                            }
                            ses = ctx.CreateSession(
                                ses.Id,
                                ses.IsAuthed,
                                ses.RememberMe,
                                ses.Lang,
                                ses.DateFmt,
                                ses.TimeFmt,
                                req.Val
                            );

                            var auth = await db.Auths.SingleOrDefaultAsync(
                                x => x.Id == ses.Id,
                                ctx.Ctkn
                            );
                            ctx.NotFoundIf(auth == null, model: new { Name = "Auth" });
                            ctx.ErrorIf(
                                auth.NotNull().ActivatedOn.IsZero(),
                                S.AuthAccountNotVerified
                            );
                            auth.FcmEnabled = req.Val;
                            await db.FcmRegs
                                .Where(x => x.User == ses.Id)
                                .ExecuteUpdateAsync(
                                    x => x.SetProperty(x => x.FcmEnabled, _ => req.Val),
                                    ctx.Ctkn
                                );
                            var fcm = ctx.Get<IFcmClient>();
                            var tokens = await db.FcmRegs
                                .Where(x => x.User == ses.Id)
                                .Select(x => x.Token)
                                .Distinct()
                                .ToListAsync(ctx.Ctkn);
                            await fcm.SendRaw(
                                ctx,
                                req.Val ? FcmType.Enabled : FcmType.Disabled,
                                tokens,
                                "",
                                null
                            );
                            return ses.ToApi();
                        }
                    )
            ),
            new RpcEndpoint<FcmRegister, FcmRegisterRes>(
                AuthRpcs.FcmRegister,
                async (ctx, req) =>
                    await ctx.DbTx<TDbCtx, FcmRegisterRes>(
                        async (db, ses) =>
                        {
                            ctx.BadRequestIf(
                                req.Topic.Count < 1 || req.Topic.Count > 5,
                                S.AuthFcmTopicInvalid,
                                new { Min = 1, Max = 5 }
                            );
                            ctx.BadRequestIf(req.Token.IsNullOrWhiteSpace(), S.AuthFcmTokenInvalid);
                            ctx.BadRequestIf(!ses.FcmEnabled, S.AuthFcmNotEnabled);
                            await _validateFcmTopic(ctx, db, ses, req.Topic);
                            var client = req.Client ?? Id.New();
                            var fcmRegs = await db.FcmRegs
                                .Where(x => x.User == ses.Id)
                                .OrderByDescending(x => x.CreatedOn)
                                .ToListAsync(ctx.Ctkn);

                            var topic = Fcm.TopicString(req.Topic);
                            var existing = fcmRegs
                                .Where(
                                    x =>
                                        (x.Topic == topic && x.Token == req.Token)
                                        || (x.Client == req.Client)
                                )
                                .ToList();
                            if (existing.Any())
                            {
                                db.FcmRegs.RemoveRange(existing);
                                await db.SaveChangesAsync(ctx.Ctkn);
                                await db.FcmRegs.AddAsync(
                                    new FcmReg()
                                    {
                                        Topic = topic,
                                        Token = req.Token,
                                        Client = client,
                                        CreatedOn = DateTimeExt.UtcNowMilli(),
                                        FcmEnabled = true,
                                        User = ses.Id
                                    },
                                    ctx.Ctkn
                                );
                            }
                            else
                            {
                                if (fcmRegs.Count > 4)
                                {
                                    // only allow a user to have 5 fcm tokens registered at any one time
                                    db.FcmRegs.RemoveRange(fcmRegs.GetRange(4, fcmRegs.Count - 4));
                                }
                                await db.FcmRegs.AddAsync(
                                    new FcmReg()
                                    {
                                        Topic = topic,
                                        Token = req.Token,
                                        User = ses.Id,
                                        Client = client,
                                        CreatedOn = DateTimeExt.UtcNowMilli(),
                                        FcmEnabled = true
                                    },
                                    ctx.Ctkn
                                );
                            }

                            return new FcmRegisterRes(client);
                        }
                    )
            ),
            new RpcEndpoint<FcmUnregister, Nothing>(
                AuthRpcs.FcmUnregister,
                async (ctx, req) =>
                    await ctx.DbTx<TDbCtx, Nothing>(
                        async (db, ses) =>
                        {
                            await db.FcmRegs
                                .Where(x => x.User == ses.Id && x.Client == req.Client)
                                .ExecuteDeleteAsync(ctx.Ctkn);
                            return Nothing.Inst;
                        }
                    )
            )
        };
    }

    public IReadOnlyList<IRpcEndpoint> Eps { get; }

    private void RateLimitAuthAttempts(IRpcCtx ctx, Auth auth)
    {
        ctx.ErrorIf(
            auth.LastSignInAttemptOn.SecondsSince() < _maxAuthAttemptsPerSecond,
            S.AuthAttemptRateLimit
        );
    }

    public static async Task<(Auth Auth, bool Created)> CreateAuth(
        IRpcCtx ctx,
        TDbCtx db,
        Register req,
        string id,
        string lang,
        string dateFmt,
        string timeFmt
    )
    {
        req = req with
        {
            // !!! ToLower all emails in all Auth api endpoints
            Email = req.Email.ToLower()
        };
        ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
        ctx.ErrorFromValidationResult(AuthValidator.Pwd(req.Pwd));
        var auth = await db.Auths.SingleOrDefaultAsync(
            x => x.Email.Equals(req.Email) || (x.NewEmail != null && x.NewEmail.Equals(req.Email)),
            ctx.Ctkn
        );
        var newCreated = false;
        if (auth == null)
        {
            newCreated = true;
            var verifyEmailCode = Crypto.String(32);
            var pwd = Crypto.HashPwd(req.Pwd);
            auth = new Auth
            {
                Id = id,
                Email = req.Email,
                VerifyEmailCodeCreatedOn = DateTime.UtcNow,
                VerifyEmailCode = verifyEmailCode,
                Lang = lang,
                DateFmt = dateFmt,
                TimeFmt = timeFmt,
                PwdVersion = pwd.PwdVersion,
                PwdSalt = pwd.PwdSalt,
                PwdHash = pwd.PwdHash,
                PwdIters = pwd.PwdIters
            };
            await db.Auths.AddAsync(auth, ctx.Ctkn);
        }

        return (auth, newCreated);
    }
}
