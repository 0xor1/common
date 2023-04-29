using Common.Server.Test;
using Common.Shared;
using Common.Shared.Auth;
using Microsoft.EntityFrameworkCore;
using ApiSession = Common.Shared.Auth.Session;

namespace Common.Server.Auth;

public interface IAuthDb
{
    DbSet<Auth> Auths { get; }
}

public class AuthEps<TDbCtx>
    where TDbCtx : DbContext, IAuthDb
{
    private readonly int _maxAuthAttemptsPerSecond;
    private readonly Func<TDbCtx, Session, Task> _onDelete;

    public AuthEps(int maxAuthAttemptsPerSecond, Func<TDbCtx, Session, Task> onDelete)
    {
        _maxAuthAttemptsPerSecond = maxAuthAttemptsPerSecond;
        _onDelete = onDelete;
        Eps = new List<IRpcEndpoint>
        {
            new RpcEndpoint<Nothing, ApiSession>(
                AuthRpcs.GetSession,
                (ctx, _) => ctx.GetSession().ToApiSession().AsTask()
            ),
            new RpcEndpoint<Register, Nothing>(
                AuthRpcs.Register,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
                    // !!! ToLower all emails in all Auth_ api endpoints
                    req = req with
                    {
                        Email = req.Email.ToLower()
                    };
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    ctx.ErrorFromValidationResult(AuthValidator.Pwd(req.Pwd));
                    return await ctx.DbTx<TDbCtx, Nothing>(
                        async (db, ses) =>
                        {
                            var existing = await db.Auths.SingleOrDefaultAsync(
                                x =>
                                    x.Email.Equals(req.Email)
                                    || (x.NewEmail != null && x.NewEmail.Equals(req.Email))
                            );
                            var newCreated = existing == null;
                            if (existing == null)
                            {
                                var verifyEmailCode = Crypto.String(32);
                                var pwd = Crypto.HashPwd(req.Pwd);
                                existing = new Auth
                                {
                                    Id = ses.Id,
                                    Email = req.Email,
                                    VerifyEmailCodeCreatedOn = DateTime.UtcNow,
                                    VerifyEmailCode = verifyEmailCode,
                                    Lang = ses.Lang,
                                    DateFmt = ses.DateFmt,
                                    TimeFmt = ses.TimeFmt,
                                    PwdVersion = pwd.PwdVersion,
                                    PwdSalt = pwd.PwdSalt,
                                    PwdHash = pwd.PwdHash,
                                    PwdIters = pwd.PwdIters
                                };
                                await db.Auths.AddAsync(existing);
                            }

                            if (
                                !existing.VerifyEmailCode.IsNullOrEmpty()
                                && (
                                    newCreated
                                    || (
                                        existing.VerifyEmailCodeCreatedOn.MinutesSince() > 10
                                        && existing.ActivatedOn.IsZero()
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
                                    existing.Email,
                                    Code = existing.VerifyEmailCode
                                };
                                await emailClient.SendEmailAsync(
                                    ctx.String(S.AuthConfirmEmailSubject),
                                    ctx.String(S.AuthConfirmEmailHtml, model),
                                    ctx.String(S.AuthConfirmEmailText, model),
                                    config.Email.NoReplyAddress,
                                    new List<string> { req.Email }
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
                                    || (x.NewEmail != null && x.NewEmail.Equals(req.Email))
                            );
                            ctx.NotFoundIf(auth == null);
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
                                x => x.Email.Equals(req.Email)
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
                            await db.SaveChangesAsync();
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
                                new List<string> { req.Email }
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
                                x => x.Email.Equals(req.Email)
                            );
                            ctx.NotFoundIf(auth == null);
                            ctx.ErrorIf(
                                auth.NotNull().ResetPwdCode != req.Code,
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
                                x => x.Email.Equals(req.Email)
                            );
                            ctx.NotFoundIf(auth == null);
                            ctx.ErrorIf(
                                auth.NotNull().ActivatedOn.IsZero(),
                                S.AuthAccountNotVerified
                            );
                            RateLimitAuthAttempts(ctx, auth.NotNull());
                            auth.LastSignInAttemptOn = DateTimeExt.UtcNowMilli();
                            var pwdIsValid = Crypto.PwdIsValid(req.Pwd, auth);
                            ctx.NotFoundIf(!pwdIsValid);
                            if (pwdIsValid)
                            {
                                auth.LastSignedInOn = DateTimeExt.UtcNowMilli();
                                ses = ctx.CreateSession(
                                    auth.Id,
                                    true,
                                    req.RememberMe,
                                    auth.Lang,
                                    auth.DateFmt,
                                    auth.TimeFmt
                                );
                            }
                            return ses.ToApiSession();
                        },
                        false
                    );
                }
            ),
            new RpcEndpoint<Nothing, ApiSession>(
                AuthRpcs.SignOut,
                (ctx, _) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    if (ses.IsAuthed)
                        ses = ctx.ClearSession();
                    return ses.ToApiSession().AsTask();
                }
            ),
            new RpcEndpoint<Nothing, ApiSession>(
                AuthRpcs.Delete,
                async (ctx, _) =>
                    await ctx.DbTx<TDbCtx, ApiSession>(
                        async (db, ses) =>
                        {
                            await _onDelete(db, ses);
                            await db.Auths.Where(x => x.Id == ses.Id).ExecuteDeleteAsync();
                            ses = ctx.ClearSession();
                            return ses.ToApiSession();
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
                        return ses.ToApiSession();

                    var s = ctx.Get<S>();
                    ses = ctx.CreateSession(
                        ses.Id,
                        ses.IsAuthed,
                        ses.RememberMe,
                        s.BestLang(req.Lang),
                        req.DateFmt,
                        req.TimeFmt
                    );
                    if (ses.IsAuthed)
                    {
                        await ctx.DbTx<TDbCtx, Nothing>(
                            async (db, _) =>
                            {
                                var auth = await db.Auths.SingleOrDefaultAsync(x => x.Id == ses.Id);
                                ctx.NotFoundIf(auth == null);
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

                    return ses.ToApiSession();
                }
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
}
