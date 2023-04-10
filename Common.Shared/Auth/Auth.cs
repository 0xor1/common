using Newtonsoft.Json;

namespace Common.Shared.Auth;

public interface IApi
{
    public IAuthApi Auth { get; }
}

public interface IAuthApi
{
    public Rpc<Nothing, Session> GetSession { get; }
    public Rpc<Register, Nothing> Register { get; }
    public Rpc<VerifyEmail, Nothing> VerifyEmail { get; }

    public Rpc<SendResetPwdEmail, Nothing> SendResetPwdEmail { get; }

    public Rpc<ResetPwd, Nothing> ResetPwd { get; }
    public Rpc<SignIn, Session> SignIn { get; }
    public Rpc<Nothing, Session> SignOut { get; }
    public Rpc<SetL10n, Session> SetL10n { get; }

    private static IAuthApi? _inst;
    public static IAuthApi Init() => _inst ??= new AuthApi();
}

internal class AuthApi : IAuthApi
{
    public Rpc<Nothing, Session> GetSession { get; } = new("/auth/get_session");
    public Rpc<Register, Nothing> Register { get; } = new("/auth/register");
    public Rpc<VerifyEmail, Nothing> VerifyEmail { get; } = new("/auth/verify_email");

    public Rpc<SendResetPwdEmail, Nothing> SendResetPwdEmail { get; } =
        new("/auth/send_reset_pwd_email");

    public Rpc<ResetPwd, Nothing> ResetPwd { get; } = new("/auth/reset_pwd");
    public Rpc<SignIn, Session> SignIn { get; } = new("/auth/sign_in");
    public Rpc<Nothing, Session> SignOut { get; } = new("/auth/sign_out");
    public Rpc<SetL10n, Session> SetL10n { get; } = new("/auth/set_l10n");
}

public interface ISession
{
    string Id { get; }
    bool IsAuthed { get; }
    bool IsAnon => !IsAuthed;
    string Lang { get; }
    string DateFmt { get; }
    string TimeFmt { get; }
}

public record Session(
    string Id,
    bool IsAuthed,
    DateTime StartedOn,
    bool RememberMe,
    string Lang,
    string DateFmt,
    string TimeFmt
) : ISession
{
    public Session(string lang, string dateFmt, string timeFmt)
        : this(string.Empty, false, DateTime.UtcNow, false, lang, dateFmt, timeFmt) { }

    [JsonIgnore]
    public bool IsAnon => !IsAuthed;
}

public record Register(string Email, string Pwd);

public record VerifyEmail(string Email, string Code);

public record SendResetPwdEmail(string Email);

public record ResetPwd(string Email, string Code, string NewPwd);

public record SignIn(string Email, string Pwd, bool RememberMe);

public record SetL10n(string Lang, string DateFmt, string TimeFmt);
