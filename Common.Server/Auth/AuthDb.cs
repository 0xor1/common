using Common.Shared;
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