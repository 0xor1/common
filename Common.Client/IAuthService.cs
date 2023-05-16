using Common.Shared.Auth;

namespace Common.Client;

public interface IAuthService
{
    void RegisterRefreshUi(Action<ISession> s);
    Task<ISession> GetSession();
    Task Register(string email, string pwd);
    Task<ISession> SignIn(string email, string pwd, bool rememberMe);
    Task<ISession> SignOut();
    Task<ISession> Delete();
    Task<ISession> SetL10n(string lang, string dateFmt, string timeFmt);
    Task FcmEnabled(bool enabled);

    public Task FcmRegister(List<string> topic);

    public Task FcmUnregister();
}
