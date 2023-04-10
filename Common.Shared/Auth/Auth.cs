using MessagePack;
using Newtonsoft.Json;

namespace Common.Shared.Auth;

public static class AuthApi
{
    public static readonly Rpc<Nothing, Session> GetSession = new("/auth/get_session");
    public static Rpc<Register, Nothing> Register { get; } = new("/auth/register");
    public static Rpc<VerifyEmail, Nothing> VerifyEmail { get; } = new("/auth/verify_email");
    public static Rpc<SendResetPwdEmail, Nothing> SendResetPwdEmail { get; } =
        new("/auth/send_reset_pwd_email");
    public static Rpc<ResetPwd, Nothing> ResetPwd { get; } = new("/auth/reset_pwd");
    public static Rpc<SignIn, Session> SignIn { get; } = new("/auth/sign_in");
    public static Rpc<Nothing, Session> SignOut { get; } = new("/auth/sign_out");
    public static Rpc<SetL10n, Session> SetL10n { get; } = new("/auth/set_l10n");
}

public record Session(
    string Id,
    bool IsAuthed,
    DateTime StartedOn,
    bool RememberMe,
    string Lang,
    string DateFmt,
    string TimeFmt
)
{
    [JsonIgnore]
    public bool IsAnon => !IsAuthed;
}

public record Register(string Email, string Pwd);

public record VerifyEmail(string Email, string Code);

public record SendResetPwdEmail(string Email);

public record ResetPwd(string Email, string Code, string NewPwd);

public record SignIn(string Email, string Pwd, bool RememberMe);

public record SetL10n(string Lang, string DateFmt, string TimeFmt);
