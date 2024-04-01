using Common.Shared;
using Common.Shared.Auth;
using Microsoft.EntityFrameworkCore;
using ApiSession = Common.Shared.Auth.Session;
using CS = Common.Shared.I18n.S;

namespace Common.Server.Auth;

public class AuthEps<TDbCtx>
    where TDbCtx : DbContext, IAuthDb
{
    private readonly int _maxAuthAttemptsPerSecond;
    private readonly bool _fcmRequiresAuth;
    private readonly int _maxFcmRegs;
    private readonly Func<IRpcCtx, TDbCtx, string, string, Task> _onActivation;
    private readonly Func<IRpcCtx, TDbCtx, ISession, Task> _onDelete;
    private readonly Func<IRpcCtx, TDbCtx, ISession, IReadOnlyList<string>, Task> _validateFcmTopic;

    public AuthEps(
        int maxAuthAttemptsPerSecond,
        bool fcmRequiresAuth,
        int maxFcmRegs,
        Func<IRpcCtx, TDbCtx, string, string, Task> onActivation,
        Func<IRpcCtx, TDbCtx, ISession, Task> onDelete,
        Func<IRpcCtx, TDbCtx, ISession, IReadOnlyList<string>, Task> validateFcmTopic
    )
    {
        Throw.DataIf(
            maxAuthAttemptsPerSecond < 0,
            $"{nameof(maxAuthAttemptsPerSecond)} must be >= 0"
        );
        Throw.DataIf(maxFcmRegs < 1, $"{nameof(maxFcmRegs)} must be >= 1");
        _maxAuthAttemptsPerSecond = maxAuthAttemptsPerSecond;
        _fcmRequiresAuth = fcmRequiresAuth;
        _maxFcmRegs = maxFcmRegs;
        _onActivation = onActivation;
        _onDelete = onDelete;
        _validateFcmTopic = validateFcmTopic;
        Eps = new List<IEp>
        {
            new Ep<Nothing, ApiSession>(
                AuthRpcs.GetSession,
                (ctx, _) => ctx.GetSession().ToApi().AsTask()
            ),
            new Ep<Register, Nothing>(
                AuthRpcs.Register,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, CS.AuthAlreadyAuthenticated);
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
                                ses.TimeFmt,
                                ses.DateSeparator,
                                ses.ThousandsSeparator,
                                ses.DecimalSeparator
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
                                    BaseHref = config.Auth.BaseHref,
                                    Email = auth.Email.UrlEncode(),
                                    Code = auth.VerifyEmailCode
                                };
                                await emailClient.SendEmailAsync(
                                    ctx.String(CS.AuthConfirmEmailSubject),
                                    ctx.String(CS.AuthConfirmEmailHtml, model),
                                    ctx.String(CS.AuthConfirmEmailText, model),
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
            new Ep<VerifyEmail, Nothing>(
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
                                CS.AuthInvalidEmailCode
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
            new Ep<SendResetPwdEmail, Nothing>(
                AuthRpcs.SendResetPwdEmail,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, CS.AuthAlreadyAuthenticated);
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
                                BaseHref = config.Auth.BaseHref,
                                Email = existing.Email.UrlEncode(),
                                Code = existing.ResetPwdCode
                            };
                            var emailClient = ctx.Get<IEmailClient>();
                            await emailClient.SendEmailAsync(
                                ctx.String(CS.AuthResetPwdSubject),
                                ctx.String(CS.AuthResetPwdHtml, model),
                                ctx.String(CS.AuthResetPwdText, model),
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
            new Ep<ResetPwd, Nothing>(
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
                                CS.AuthInvalidResetPwdCode
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
            new Ep<SendMagicLinkEmail, Nothing>(
                AuthRpcs.SendMagicLinkEmail,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, CS.AuthAlreadyAuthenticated);
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
                                BaseHref = config.Auth.BaseHref,
                                Email = auth.Email.UrlEncode(),
                                Code = auth.MagicLinkCode,
                                req.RememberMe
                            };
                            var emailClient = ctx.Get<IEmailClient>();
                            await emailClient.SendEmailAsync(
                                ctx.String(CS.AuthMagicLinkSubject),
                                ctx.String(CS.AuthMagicLinkHtml, model),
                                ctx.String(CS.AuthMagicLinkText, model),
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
            new Ep<MagicLinkSignIn, ApiSession>(
                AuthRpcs.MagicLinkSignIn,
                async (ctx, req) =>
                {
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, CS.AuthAlreadyAuthenticated);
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
                                CS.AuthAccountNotVerified
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
                                    auth.DateSeparator,
                                    auth.ThousandsSeparator,
                                    auth.DecimalSeparator,
                                    auth.FcmEnabled
                                )
                                .ToApi();
                        },
                        false
                    );
                }
            ),
            new Ep<SignIn, ApiSession>(
                AuthRpcs.SignIn,
                async (ctx, req) =>
                {
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, CS.AuthAlreadyAuthenticated);
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
                                CS.AuthAccountNotVerified
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
                                    auth.DateSeparator,
                                    auth.ThousandsSeparator,
                                    auth.DecimalSeparator,
                                    auth.FcmEnabled
                                )
                                .ToApi();
                        },
                        false
                    );
                }
            ),
            new Ep<Nothing, ApiSession>(
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
            Ep<Nothing, ApiSession>.DbTx<TDbCtx>(
                AuthRpcs.Delete,
                async (ctx, db, ses, _) =>
                {
                    await _onDelete(ctx, db, ses);
                    await db.Auths.Where(x => x.Id == ses.Id).ExecuteDeleteAsync(ctx.Ctkn);
                    await db.FcmRegs.Where(x => x.User == ses.Id).ExecuteDeleteAsync(ctx.Ctkn);
                    ses = ctx.ClearSession();
                    return ses.ToApi();
                }
            ),
            new Ep<SetL10n, ApiSession>(
                AuthRpcs.SetL10n,
                async (ctx, req) =>
                {
                    ctx.BadRequestIf(
                        req.Lang.IsNullOrEmpty()
                            || req.TimeFmt.IsNullOrEmpty()
                            || req.DateSeparator.IsNullOrEmpty()
                            || req.ThousandsSeparator.IsNullOrEmpty()
                            || req.DecimalSeparator.IsNullOrEmpty()
                    );
                    ctx.BadRequestIf(
                        req.ThousandsSeparator == req.DecimalSeparator,
                        CS.AuthThousandsAndDecimalSeparatorsMatch
                    );
                    var ses = ctx.GetSession();
                    if (
                        ses.Lang == req.Lang
                        && ses.DateFmt == req.DateFmt
                        && ses.TimeFmt == req.TimeFmt
                        && ses.DateSeparator == req.DateSeparator
                        && ses.ThousandsSeparator == req.ThousandsSeparator
                        && ses.DecimalSeparator == req.DecimalSeparator
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
                        req.DateSeparator,
                        req.ThousandsSeparator,
                        req.DecimalSeparator,
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
                                    CS.AuthAccountNotVerified
                                );
                                auth.Lang = ses.Lang;
                                auth.DateFmt = ses.DateFmt;
                                auth.TimeFmt = ses.TimeFmt;
                                auth.DateSeparator = ses.DateSeparator;
                                auth.ThousandsSeparator = ses.ThousandsSeparator;
                                auth.DecimalSeparator = ses.DecimalSeparator;
                                return Nothing.Inst;
                            }
                        );
                    }

                    return ses.ToApi();
                }
            ),
            Ep<FcmEnabled, ApiSession>.DbTx<TDbCtx>(
                AuthRpcs.FcmEnabled,
                async (ctx, db, ses, req) =>
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
                        ses.DateSeparator,
                        ses.ThousandsSeparator,
                        ses.DecimalSeparator,
                        req.Val
                    );

                    var auth = await db.Auths.SingleOrDefaultAsync(x => x.Id == ses.Id, ctx.Ctkn);
                    ctx.NotFoundIf(auth == null, model: new { Name = "Auth" });
                    ctx.ErrorIf(auth.NotNull().ActivatedOn.IsZero(), CS.AuthAccountNotVerified);
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
                },
                _fcmRequiresAuth
            ),
            Ep<FcmRegister, FcmRegisterRes>.DbTx<TDbCtx>(
                AuthRpcs.FcmRegister,
                async (ctx, db, ses, req) =>
                {
                    ctx.BadRequestIf(
                        req.Topic.Count < 1 || req.Topic.Count > 5,
                        CS.AuthFcmTopicInvalid,
                        new { Min = 1, Max = 5 }
                    );
                    ctx.BadRequestIf(req.Token.IsNullOrWhiteSpace(), CS.AuthFcmTokenInvalid);
                    ctx.BadRequestIf(!ses.FcmEnabled, CS.AuthFcmNotEnabled);
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
                        if (fcmRegs.Count > _maxFcmRegs - 1)
                        {
                            db.FcmRegs.RemoveRange(
                                fcmRegs.GetRange(_maxFcmRegs - 1, fcmRegs.Count - (_maxFcmRegs - 1))
                            );
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
                },
                _fcmRequiresAuth
            ),
            Ep<FcmUnregister, Nothing>.DbTx<TDbCtx>(
                AuthRpcs.FcmUnregister,
                async (ctx, db, ses, req) =>
                {
                    await db.FcmRegs
                        .Where(x => x.User == ses.Id && x.Client == req.Client)
                        .ExecuteDeleteAsync(ctx.Ctkn);
                    return Nothing.Inst;
                },
                _fcmRequiresAuth
            )
        };
    }

    public IReadOnlyList<IEp> Eps { get; }

    private void RateLimitAuthAttempts(IRpcCtx ctx, Auth auth)
    {
        ctx.ErrorIf(
            auth.LastSignInAttemptOn.SecondsSince() < _maxAuthAttemptsPerSecond,
            CS.AuthAttemptRateLimit
        );
    }

    public static async Task<(Auth Auth, bool Created)> CreateAuth(
        IRpcCtx ctx,
        TDbCtx db,
        Register req,
        string id,
        string lang,
        DateFmt dateFmt,
        string timeFmt,
        string dateSeparator,
        string thousandsSeparator,
        string decimalSeparator
    )
    {
        req = req with
        {
            // !!! ToLower all emails in all Auth api endpoints
            Email = req.Email.ToLower()
        };
        ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
        ctx.ErrorFromValidationResult(AuthValidator.Pwd(req.Pwd));
        ctx.BadRequestIf(
            thousandsSeparator == decimalSeparator,
            CS.AuthThousandsAndDecimalSeparatorsMatch
        );
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
                DateSeparator = dateSeparator,
                ThousandsSeparator = thousandsSeparator,
                DecimalSeparator = decimalSeparator,
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
