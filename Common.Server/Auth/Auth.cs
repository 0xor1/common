using Common.Shared;
using Microsoft.EntityFrameworkCore;

namespace Common.Server.Auth;

[PrimaryKey(nameof(Id))]
public class Auth : Pwd
{
    public string Id { get; set; }
    public string Email { get; set; }
    public DateTime LastSignedInOn { get; set; } = DateTimeExt.Zero();
    public DateTime LastSignInAttemptOn { get; set; } = DateTimeExt.Zero();
    public DateTime ActivatedOn { get; set; } = DateTimeExt.Zero();
    public string? NewEmail { get; set; }
    public DateTime VerifyEmailCodeCreatedOn { get; set; } = DateTimeExt.UtcNowMilli();
    public string VerifyEmailCode { get; set; } = "";
    public DateTime ResetPwdCodeCreatedOn { get; set; } = DateTimeExt.Zero();
    public string ResetPwdCode { get; set; } = "";
    public DateTime MagicLinkCodeCreatedOn { get; set; } = DateTimeExt.Zero();
    public string MagicLinkCode { get; set; } = "";
    public bool Use2FA { get; set; }
    public string Lang { get; set; }
    public DateFmt DateFmt { get; set; }
    public string TimeFmt { get; set; }
    public string DateSeparator { get; set; }
    public string ThousandsSeparator { get; set; }
    public string DecimalSeparator { get; set; }
    public bool FcmEnabled { get; set; }
}
