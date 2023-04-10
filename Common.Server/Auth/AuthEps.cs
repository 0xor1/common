using Common.Shared;
using Common.Shared.Auth;
using Microsoft.EntityFrameworkCore;
using ApiSession = Common.Shared.Auth.Session;

namespace Common.Server.Auth;

public static class AuthEps
{
    public static IReadOnlyList<IRpcEndpoint> Eps { get; } =
        new List<IRpcEndpoint>()
        {
            new RpcEndpoint<Nothing, ApiSession>(
                AuthApi.GetSession,
                (ctx, req) => ctx.GetSession().ToApiSession().Task()
            ),
            new RpcEndpoint<Register, Nothing>(
                AuthApi.Register,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
                    // !!! ToLower all emails in all Auth_ api endpoints
                    req = req with {Email = req.Email.ToLower()};
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    ctx.ErrorFromValidationResult(AuthValidator.Pwd(req.Pwd));
                    var db = ctx.GetRequiredService<AuthDb>();
                    // start db tx
                    await using var tx = await db.Database.BeginTransactionAsync();
                    try
                    {
                        var existing = await db.Auths.SingleOrDefaultAsync(
                            x => x.Email.Equals(req.Email) || (x.NewEmail != null && x.NewEmail.Equals(req.Email))
                        );
                        var newCreated = existing == null;
                        if (existing == null)
                        {
                            var verifyEmailCode = Crypto.String(32);
                            var pwd = Crypto.HashPwd(req.Pwd);
                            existing = new Auth()
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
                            var config = ctx.GetRequiredService<IConfig>();
                            var emailClient = ctx.GetRequiredService<IEmailClient>();
                            var model = new
                            {
                                BaseHref = config.Server.Listen,
                                Email = existing.Email,
                                Code = existing.VerifyEmailCode
                            };
                            await emailClient.SendEmailAsync(
                                ctx.String(S.AuthConfirmEmailSubject),
                                ctx.String(S.AuthConfirmEmailHtml, model),
                                ctx.String(S.AuthConfirmEmailText, model),
                                config.Email.NoReplyAddress,
                                new List<string>() { req.Email }
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
            new RpcEndpoint<VerifyEmail, Nothing>(AuthApi.VerifyEmail, async (ctx, req) =>
            {
                // !!! ToLower all emails in all Auth_ api endpoints
                req = req with { Email = req.Email.ToLower() };
                ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                var db = ctx.GetRequiredService<AuthDb>();
                // start db tx
                await using var tx = await db.Database.BeginTransactionAsync();
                try
                {
                    var auth = await db.Auths.SingleOrDefaultAsync(
                        x => x.Email.Equals(req.Email) || (x.NewEmail != null && x.NewEmail.Equals(req.Email))
                    );
                    ctx.ErrorIf(auth == null, S.NoMatchingRecord);
                    ctx.ErrorIf(auth.NotNull().VerifyEmailCode != req.Code, S.AuthInvalidEmailCode);
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
            }),
            new RpcEndpoint<SendResetPwdEmail, Nothing>(AuthApi.SendResetPwdEmail, async (ctx, req) =>
            {
                // TODO
            }),
            new RpcEndpoint<ResetPwd, Nothing>(AuthApi.ResetPwd, async (ctx, req) =>
            {
                // TODO
            }),
            new RpcEndpoint<SignIn, ApiSession>(AuthApi.SignIn, async (ctx, req) =>
            {
                // TODO
            }),
            new RpcEndpoint<Nothing, ApiSession>(AuthApi.SignOut, async (ctx, req) =>
            {
                // TODO
            }),
            new RpcEndpoint<SetL10n, ApiSession>(AuthApi.SetL10n, async (ctx, req) =>
            {
                // TODO
            })
        };
}
