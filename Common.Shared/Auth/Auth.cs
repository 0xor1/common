using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Common.Shared.Auth;

public interface IApi
{
    public IAuthApi Auth { get; }
}

public static class Fcm
{
    public const string ClientHeaderName = "X-Fcm-Client";
    public const string TypeName = "X-Fcm-Type";
    public const string Data = "Data";
}

public interface IAuthApi
{
    Task<Session> GetSession();
    Task Register(Register arg);
    Task VerifyEmail(VerifyEmail arg);
    Task SendResetPwdEmail(SendResetPwdEmail arg);
    Task ResetPwd(ResetPwd arg);
    Task<Session> SignIn(SignIn arg);
    Task<Session> SignOut();
    Task<Session> Delete();
    Task<Session> SetL10n(SetL10n arg);
    public Task<Session> FcmEnabled(FcmEnabled arg);
    public Task<FcmRegisterRes> FcmRegister(FcmRegister arg);
    public Task FcmUnregister(FcmUnregister arg);
}

public class Api : IApi
{
    public Api(IRpcClient client)
    {
        Auth = new AuthApi(client);
    }

    public IAuthApi Auth { get; }
}

public class AuthApi : IAuthApi
{
    private readonly IRpcClient _client;

    public AuthApi(IRpcClient client)
    {
        _client = client;
    }

    public Task<Session> GetSession() => _client.Do(AuthRpcs.GetSession, Nothing.Inst);

    public Task Register(Register arg) => _client.Do(AuthRpcs.Register, arg);

    public Task VerifyEmail(VerifyEmail arg) => _client.Do(AuthRpcs.VerifyEmail, arg);

    public Task SendResetPwdEmail(SendResetPwdEmail arg) =>
        _client.Do(AuthRpcs.SendResetPwdEmail, arg);

    public Task ResetPwd(ResetPwd arg) => _client.Do(AuthRpcs.ResetPwd, arg);

    public Task<Session> SignIn(SignIn arg) => _client.Do(AuthRpcs.SignIn, arg);

    public Task<Session> SignOut() => _client.Do(AuthRpcs.SignOut, Nothing.Inst);

    public Task<Session> Delete() => _client.Do(AuthRpcs.Delete, Nothing.Inst);

    public Task<Session> SetL10n(SetL10n arg) => _client.Do(AuthRpcs.SetL10n, arg);

    public Task<Session> FcmEnabled(FcmEnabled arg) => _client.Do(AuthRpcs.FcmEnabled, arg);

    public Task<FcmRegisterRes> FcmRegister(FcmRegister arg) =>
        _client.Do(AuthRpcs.FcmRegister, arg);

    public Task FcmUnregister(FcmUnregister arg) => _client.Do(AuthRpcs.FcmUnregister, arg);
}

public static class AuthRpcs
{
    public static readonly Rpc<Nothing, Session> GetSession = new("/auth/get_session");
    public static readonly Rpc<Register, Nothing> Register = new("/auth/register");
    public static readonly Rpc<VerifyEmail, Nothing> VerifyEmail = new("/auth/verify_email");
    public static readonly Rpc<SendResetPwdEmail, Nothing> SendResetPwdEmail =
        new("/auth/send_reset_pwd_email");
    public static readonly Rpc<ResetPwd, Nothing> ResetPwd = new("/auth/reset_pwd");
    public static readonly Rpc<SignIn, Session> SignIn = new("/auth/sign_in");
    public static readonly Rpc<Nothing, Session> SignOut = new("/auth/sign_out");
    public static readonly Rpc<Nothing, Session> Delete = new("/auth/delete");
    public static readonly Rpc<SetL10n, Session> SetL10n = new("/auth/set_l10n");
    public static readonly Rpc<FcmEnabled, Session> FcmEnabled = new("/auth/fcm_enabled");
    public static readonly Rpc<FcmRegister, FcmRegisterRes> FcmRegister = new("/auth/fcm_register");
    public static readonly Rpc<FcmUnregister, Nothing> FcmUnregister = new("/auth/fcm_unregister");
}

public interface ISession
{
    string Id { get; }
    bool IsAuthed { get; }
    bool IsAnon => !IsAuthed;
    string Lang { get; }
    string DateFmt { get; }
    string TimeFmt { get; }
    bool FcmEnabled { get; }
}

public record Session(
    string Id,
    bool IsAuthed,
    DateTime StartedOn,
    bool RememberMe,
    string Lang,
    string DateFmt,
    string TimeFmt,
    bool FcmEnabled
) : ISession
{
    public static Session Default(string lang, string dateFmt, string timeFmt) =>
        new(string.Empty, false, DateTime.UtcNow, false, lang, dateFmt, timeFmt, false);

    [JsonIgnore]
    public bool IsAnon => !IsAuthed;
}

public record Register(string Email, string Pwd);

public record VerifyEmail(string Email, string Code);

public record SendResetPwdEmail(string Email);

public record ResetPwd(string Email, string Code, string NewPwd);

public record SignIn(string Email, string Pwd, bool RememberMe);

public record SetL10n(string Lang, string DateFmt, string TimeFmt);

public record FcmEnabled(bool Val);

public record FcmRegister(IReadOnlyList<string> Topic, string Token, string? Client);

public record FcmUnregister(string Client);

public record FcmRegisterRes(string Client);

public enum FcmType
{
    [EnumMember(Value = "data")]
    [Description("data")]
    Data,

    [EnumMember(Value = "signOut")]
    [Description("signOut")]
    SignOut,

    [EnumMember(Value = "enabled")]
    [Description("enabled")]
    Enabled,

    [EnumMember(Value = "disabled")]
    [Description("disabled")]
    Disabled
}
