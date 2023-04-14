using Common.Client;
using Common.Shared;
using Common.Shared.Auth;

namespace Common.Client;

public class AuthService<TApi> : IAuthService
    where TApi : class, IApi
{
    private readonly L L;
    private Session? _ses;
    private Session? Session
    {
        get => _ses;
        set
        {
            _ses = value;
            L.Config(_ses.Lang, _ses.DateFmt, _ses.TimeFmt);
            _refreshUI?.Invoke(_ses);
        }
    }

    private readonly TApi _api;
    private Action<ISession>? _refreshUI;

    public AuthService(TApi api, L l)
    {
        _api = api;
        L = l;
    }

    public void RegisterRefreshUi(Action<ISession> a)
    {
        _refreshUI = a;
    }

    public async Task<ISession> GetSession() =>
        Session ??= await _api.Auth.GetSession.Do(new Nothing());

    public async Task Register(string email, string pwd)
    {
        var ses = await GetSession();
        Throw.OpIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
        await _api.Auth.Register.Do(new(email, pwd));
    }

    public async Task<ISession> SignIn(string email, string pwd, bool rememberMe)
    {
        var ses = await GetSession();
        Throw.OpIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
        return Session = await _api.Auth.SignIn.Do(new(email, pwd, rememberMe));
    }

    public async Task<ISession> SignOut()
    {
        var ses = await GetSession();
        if (!ses.IsAuthed)
        {
            return ses;
        }
        return Session = await _api.Auth.SignOut.Do(new Nothing());
    }

    public async Task<ISession> SetL10n(string lang, string dateFmt, string timeFmt) =>
        Session = await _api.Auth.SetL10n.Do(new(lang, dateFmt, timeFmt));
}
