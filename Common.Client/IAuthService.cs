using Common.Shared.Auth;

namespace Common.Client;

public interface IAuthService
{
    void OnSessionChanged(Action<ISession> s);
    Task<ISession> GetSession(CancellationToken ctkn = default);
    Task Register(string email, string pwd, CancellationToken ctkn = default);
    Task<ISession> SignIn(
        string email,
        string pwd,
        bool rememberMe,
        CancellationToken ctkn = default
    );
    Task<ISession> SignOut(CancellationToken ctkn = default);
    Task<ISession> Delete(CancellationToken ctkn = default);
    Task<ISession> SetL10n(
        string lang,
        string dateFmt,
        string timeFmt,
        CancellationToken ctkn = default
    );
    Task<ISession> FcmEnabled(bool enabled, CancellationToken ctkn = default);

    public Task FcmRegister(List<string> topic, Action<string> a, CancellationToken ctkn = default);

    public Task FcmUnregister(CancellationToken ctkn = default);
}
