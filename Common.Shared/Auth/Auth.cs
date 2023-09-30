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
    public const string Topic = "Topic";

    public static string TopicString(IReadOnlyList<string> topic) => string.Join(":", topic);
}

public interface IAuthApi
{
    Task<Session> GetSession(CancellationToken ctkn = default);
    Task Register(Register arg, CancellationToken ctkn = default);
    Task VerifyEmail(VerifyEmail arg, CancellationToken ctkn = default);
    Task SendResetPwdEmail(SendResetPwdEmail arg, CancellationToken ctkn = default);
    Task ResetPwd(ResetPwd arg, CancellationToken ctkn = default);
    Task<Session> SignIn(SignIn arg, CancellationToken ctkn = default);
    Task<Session> SignOut(CancellationToken ctkn = default);
    Task<Session> Delete(CancellationToken ctkn = default);
    Task<Session> SetL10n(SetL10n arg, CancellationToken ctkn = default);
    public Task<Session> FcmEnabled(FcmEnabled arg, CancellationToken ctkn = default);
    public Task<FcmRegisterRes> FcmRegister(FcmRegister arg, CancellationToken ctkn = default);
    public Task FcmUnregister(FcmUnregister arg, CancellationToken ctkn = default);
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

    public Task<Session> GetSession(CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.GetSession, Nothing.Inst, ctkn);

    public Task Register(Register arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.Register, arg, ctkn);

    public Task VerifyEmail(VerifyEmail arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.VerifyEmail, arg, ctkn);

    public Task SendResetPwdEmail(SendResetPwdEmail arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.SendResetPwdEmail, arg, ctkn);

    public Task ResetPwd(ResetPwd arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.ResetPwd, arg, ctkn);

    public Task<Session> SignIn(SignIn arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.SignIn, arg, ctkn);

    public Task<Session> SignOut(CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.SignOut, Nothing.Inst, ctkn);

    public Task<Session> Delete(CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.Delete, Nothing.Inst, ctkn);

    public Task<Session> SetL10n(SetL10n arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.SetL10n, arg, ctkn);

    public Task<Session> FcmEnabled(FcmEnabled arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.FcmEnabled, arg, ctkn);

    public Task<FcmRegisterRes> FcmRegister(FcmRegister arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.FcmRegister, arg, ctkn);

    public Task FcmUnregister(FcmUnregister arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.FcmUnregister, arg, ctkn);
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
