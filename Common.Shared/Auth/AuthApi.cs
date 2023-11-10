namespace Common.Shared.Auth;

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

    public Task SendMagicLinkEmail(SendMagicLinkEmail arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.SendMagicLinkEmail, arg, ctkn);

    public Task<Session> MagicLinkSignIn(MagicLinkSignIn arg, CancellationToken ctkn = default) =>
        _client.Do(AuthRpcs.MagicLinkSignIn, arg, ctkn);

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
