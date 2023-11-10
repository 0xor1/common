namespace Common.Shared.Auth;

public interface IAuthApi
{
    Task<Session> GetSession(CancellationToken ctkn = default);
    Task Register(Register arg, CancellationToken ctkn = default);
    Task VerifyEmail(VerifyEmail arg, CancellationToken ctkn = default);
    Task SendResetPwdEmail(SendResetPwdEmail arg, CancellationToken ctkn = default);
    Task ResetPwd(ResetPwd arg, CancellationToken ctkn = default);
    Task SendMagicLinkEmail(SendMagicLinkEmail arg, CancellationToken ctkn = default);
    Task<Session> MagicLinkSignIn(MagicLinkSignIn arg, CancellationToken ctkn = default);
    Task<Session> SignIn(SignIn arg, CancellationToken ctkn = default);
    Task<Session> SignOut(CancellationToken ctkn = default);
    Task<Session> Delete(CancellationToken ctkn = default);
    Task<Session> SetL10n(SetL10n arg, CancellationToken ctkn = default);
    public Task<Session> FcmEnabled(FcmEnabled arg, CancellationToken ctkn = default);
    public Task<FcmRegisterRes> FcmRegister(FcmRegister arg, CancellationToken ctkn = default);
    public Task FcmUnregister(FcmUnregister arg, CancellationToken ctkn = default);
}
