using Common.Shared;
using Common.Shared.Auth;
using Microsoft.EntityFrameworkCore;

namespace Common.Server.Auth;

public class AuthDb : DbContext
{
    public AuthDb(DbContextOptions<AuthDb> opts)
        : base(opts) { }

    public DbSet<Auth> Auths { get; set; } = null!;
}

public class Auth : Pwd
{
    public string Id { get; set; }
    public string Email { get; set; }
    public DateTime LastSignedInOn { get; set; } = DateTimeExts.Zero();
    public DateTime LastSignInAttemptOn { get; set; } = DateTimeExts.Zero();
    public DateTime ActivatedOn { get; set; } = DateTimeExts.Zero();
    public string? NewEmail { get; set; }
    public DateTime VerifyEmailCodeCreatedOn { get; set; } = DateTime.UtcNow;
    public string VerifyEmailCode { get; set; } = "";
    public DateTime ResetPwdCodeCreatedOn { get; set; } = DateTimeExts.Zero();
    public string ResetPwdCode { get; set; } = "";
    public DateTime LoginCodeCreatedOn { get; set; } = DateTimeExts.Zero();
    public string LoginCode { get; set; } = "";
    public bool Use2FA { get; set; } = false;
    public string Lang { get; set; }
    public string DateFmt { get; set; }
    public string TimeFmt { get; set; }
}

public static class AuthEps
{
    public static IReadOnlyList<IRpcEndpoint> Eps { get; } =
        new List<IRpcEndpoint>()
        {
            new RpcEndpoint<Nothing, Common.Shared.Auth.Session>(
                AuthApi.GetSession,
                (ctx, req) => ctx.GetSession().ToApiSession().Task()
            ),
            new RpcEndpoint<RegisterReq, Nothing>(
                AuthApi.Register,
                async (ctx, req) =>
                {
                    // basic validation
                    var ses = ctx.GetSession();
                    ctx.ErrorIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
                    // !!! ToLower all emails in all Auth_ api endpoints
                    var email = req.Email.ToLower();
                    ctx.ErrorFromValidationResult(AuthValidator.Email(req.Email));
                    ctx.ErrorFromValidationResult(AuthValidator.Pwd(req.Pwd));
                    var db = ctx.GetRequiredService<AuthDb>();
                    // start db tx
                    await using var tx = await db.Database.BeginTransactionAsync();
                    try
                    {
                        var existing = await db.Auths.SingleOrDefaultAsync(
                            x => x.Email.Equals(req.Email) || x.NewEmail.Equals(req.Email)
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
            )
        };
}
