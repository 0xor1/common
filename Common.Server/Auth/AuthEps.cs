using Common.Shared;
using Common.Shared.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ApiSession = Common.Shared.Auth.Session;

namespace Common.Server.Auth;

public interface IAuthDb
{
    DatabaseFacade Database { get; }
    DbSet<Auth> Auths { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public static class AuthEps<TDbCtx>
    where TDbCtx : IAuthDb
{
    private static readonly IAuthApi Api = IAuthApi.Init();
    private const int AuthAttemptsRateLimit = 5;

    public static IReadOnlyList<IRpcEndpoint> Eps { get; } =
        new List<IRpcEndpoint>
        {
            new RpcEndpoint<Nothing, ApiSession>(
                Api.GetSession,
                (ctx, req) => ctx.GetSession().ToApiSession().AsTask()
            ),
            new RpcEndpoint<Register, Nothing>(
                Api.Register,
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
                    var db = ctx.Get<TDbCtx>();
                    // start db tx
                    await using var tx = await db.Database.BeginTransactionAsync();
                    try
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
                            await db.SaveChangesAsync();
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

                        await tx.CommitAsync();
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }

                    return new Nothing();
                }
            ),
            new RpcEndpoint<VerifyEmail, Nothing>(
                Api.VerifyEmail,
                async (ctx, req) =>
                {
                    // !!! ToLower all emails in all Auth_ api endpoints
                    req = req with
                    {
                        Email = req.Email.ToLower()
                    };
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    var db = ctx.Get<TDbCtx>();
                    // start db tx
                    await using var tx = await db.Database.BeginTransactionAsync();
                    try
                    {
                        var auth = await db.Auths.SingleOrDefaultAsync(
                            x =>
                                x.Email.Equals(req.Email)
                                || (x.NewEmail != null && x.NewEmail.Equals(req.Email))
                        );
                        ctx.ErrorIf(auth == null, S.NoMatchingRecord);
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
                            auth.ActivatedOn = DateTime.UtcNow;
                        }

                        auth.VerifyEmailCodeCreatedOn = DateTimeExts.Zero();
                        auth.VerifyEmailCode = string.Empty;
                        await db.SaveChangesAsync();
                        await tx.CommitAsync();
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }

                    return new Nothing();
                }
            ),
            new RpcEndpoint<SendResetPwdEmail, Nothing>(
                Api.SendResetPwdEmail,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
                    // !!! ToLower all emails in all Auth_ api endpoints
                    req = new SendResetPwdEmail(req.Email.ToLower());
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    var db = ctx.Get<TDbCtx>();
                    // start db tx
                    await using var tx = await db.Database.BeginTransactionAsync();
                    try
                    {
                        var existing = await db.Auths.SingleOrDefaultAsync(
                            x => x.Email.Equals(req.Email)
                        );
                        if (existing == null || existing.ResetPwdCodeCreatedOn.MinutesSince() < 10)
                            // if email is not associated with an account or
                            // a reset pwd was sent within the last 10 minutes
                            // dont do anything
                            return new Nothing();

                        existing.ResetPwdCodeCreatedOn = DateTime.UtcNow;
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
                        await tx.CommitAsync();
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }

                    return new Nothing();
                }
            ),
            new RpcEndpoint<ResetPwd, Nothing>(
                Api.ResetPwd,
                async (ctx, req) =>
                {
                    // !!! ToLower all emails in all Auth_ api endpoints
                    req = req with
                    {
                        Email = req.Email.ToLower()
                    };
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    ctx.ErrorFromValidationResult(AuthValidator.Pwd(req.NewPwd));
                    var db = ctx.Get<TDbCtx>();
                    // start db tx
                    await using var tx = await db.Database.BeginTransactionAsync();
                    try
                    {
                        var auth = await db.Auths.SingleOrDefaultAsync(
                            x => x.Email.Equals(req.Email)
                        );
                        ctx.ErrorIf(auth == null, S.NoMatchingRecord);
                        ctx.ErrorIf(
                            auth.NotNull().ResetPwdCode != req.Code,
                            S.AuthInvalidResetPwdCode
                        );
                        var pwd = Crypto.HashPwd(req.NewPwd);
                        auth.ResetPwdCodeCreatedOn = DateTimeExts.Zero();
                        auth.ResetPwdCode = string.Empty;
                        auth.PwdVersion = pwd.PwdVersion;
                        auth.PwdSalt = pwd.PwdSalt;
                        auth.PwdHash = pwd.PwdHash;
                        auth.PwdIters = pwd.PwdIters;
                        await db.SaveChangesAsync();
                        await tx.CommitAsync();
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }

                    return new Nothing();
                }
            ),
            new RpcEndpoint<SignIn, ApiSession>(
                Api.SignIn,
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
                    var db = ctx.Get<TDbCtx>();
                    // start db tx
                    await using var tx = await db.Database.BeginTransactionAsync();
                    var auth = await db.Auths.SingleOrDefaultAsync(x => x.Email.Equals(req.Email));
                    ctx.ErrorIf(auth == null, S.NoMatchingRecord);
                    ctx.ErrorIf(auth.NotNull().ActivatedOn.IsZero(), S.AuthAccountNotVerified);
                    RateLimitAuthAttempts(ctx, auth.NotNull());
                    auth.LastSignInAttemptOn = DateTime.UtcNow;
                    var pwdIsValid = Crypto.PwdIsValid(req.Pwd, auth);
                    if (pwdIsValid)
                    {
                        auth.LastSignedInOn = DateTime.UtcNow;
                        ses = ctx.CreateSession(
                            auth.Id,
                            true,
                            req.RememberMe,
                            auth.Lang,
                            auth.DateFmt,
                            auth.TimeFmt
                        );
                    }

                    await db.SaveChangesAsync();
                    await tx.CommitAsync();
                    ctx.ErrorIf(!pwdIsValid, S.NoMatchingRecord);
                    return ses.ToApiSession();
                }
            ),
            new RpcEndpoint<Nothing, ApiSession>(
                Api.SignOut,
                (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    if (ses.IsAuthed)
                        ses = ctx.ClearSession();
                    return ses.ToApiSession().AsTask();
                }
            ),
            new RpcEndpoint<SetL10n, ApiSession>(
                Api.SetL10n,
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
                        var db = ctx.Get<TDbCtx>();
                        await using var tx = await db.Database.BeginTransactionAsync();
                        var auth = await db.Auths.SingleOrDefaultAsync(x => x.Id.Equals(ses.Id));
                        ctx.ErrorIf(auth == null, S.NoMatchingRecord);
                        ctx.ErrorIf(auth.NotNull().ActivatedOn.IsZero(), S.AuthAccountNotVerified);
                        auth.Lang = ses.Lang;
                        auth.DateFmt = ses.DateFmt;
                        auth.TimeFmt = ses.TimeFmt;
                        await db.SaveChangesAsync();
                        await tx.CommitAsync();
                    }

                    return ses.ToApiSession();
                }
            )
        };

    private static void RateLimitAuthAttempts(HttpContext ctx, Auth auth)
    {
        ctx.ErrorIf(
            auth.LastSignInAttemptOn.SecondsSince() < AuthAttemptsRateLimit,
            S.AuthAttemptRateLimit
        );
    }
}
