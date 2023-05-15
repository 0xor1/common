using Common.Shared;
using Common.Shared.Auth;
using Microsoft.JSInterop;

namespace Common.Client;

public class AuthService<TApi> : IAuthService
    where TApi : class, IApi
{
    private readonly IJSRuntime _js;
    private bool _fcmEnabled = false;
    private string? _fcmToken = null;
    private string? _fcmClient = null;
    private readonly L L;
    private Session? _ses;

    public AuthService(IJSRuntime js)
    {
        _js = js;
    }
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

    public async Task<ISession> GetSession() => Session ??= await _api.Auth.GetSession();

    public async Task Register(string email, string pwd)
    {
        var ses = await GetSession();
        Throw.OpIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
        await _api.Auth.Register(new(email, pwd));
    }

    public async Task<ISession> SignIn(string email, string pwd, bool rememberMe)
    {
        var ses = await GetSession();
        Throw.OpIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
        return Session = await _api.Auth.SignIn(new(email, pwd, rememberMe));
    }

    public async Task<ISession> SignOut()
    {
        var ses = await GetSession();
        if (!ses.IsAuthed)
        {
            return ses;
        }

        return Session = await _api.Auth.SignOut();
    }

    public async Task<ISession> Delete()
    {
        var ses = await GetSession();
        if (!ses.IsAuthed)
        {
            return ses;
        }

        return Session = await _api.Auth.Delete();
    }

    public async Task<ISession> SetL10n(string lang, string dateFmt, string timeFmt) =>
        Session = await _api.Auth.SetL10n(new(lang, dateFmt, timeFmt));

    public async Task<ISession> FcmEnabled(bool enabled)
    {
        // todo call jsinterop to ask for notification permission and get a fcm token
        Session = await _api.Auth.FcmEnabled(new(enabled));
        return Session;
    }

    public async Task FcmRegister(List<string> topic)
    {
        if (_fcmEnabled && !_fcmClient.IsNullOrEmpty() && !_fcmToken.IsNullOrEmpty())
        {
            _fcmClient = (await _api.Auth.FcmRegister(new(topic, _fcmToken, _fcmClient))).Client;
        }
    }

    public async Task FcmUnregister()
    {
        if (!_fcmClient.IsNullOrEmpty())
        {
            await _api.Auth.FcmUnregister(new(_fcmClient));
        }
    }
}
