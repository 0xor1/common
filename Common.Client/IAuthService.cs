using Common.Shared;
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
    Task<ISession> MagicLinkSignIn(
        string email,
        string code,
        bool rememberMe,
        CancellationToken ctkn = default
    );
    Task<ISession> SignOut(CancellationToken ctkn = default);
    Task<ISession> Delete(CancellationToken ctkn = default);
    Task<ISession> SetL10n(
        string lang,
        DateFmt dateFmt,
        string timeFmt,
        string dateSeparator,
        string thousandsSeparator,
        string decimalSeparator,
        CancellationToken ctkn = default
    );
    Task<ISession> FcmEnabled(bool enabled, CancellationToken ctkn = default);

    public Task FcmRegister(List<string> topic, Action<string> a, CancellationToken ctkn = default);

    public Task FcmUnregister(CancellationToken ctkn = default);
}
