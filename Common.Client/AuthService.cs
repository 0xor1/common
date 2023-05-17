using Common.Shared;
using Common.Shared.Auth;
using Microsoft.JSInterop;

namespace Common.Client;

public class AuthService<TApi> : IAuthService, IDisposable
    where TApi : class, IApi
{
    private readonly IJSRuntime _js;
    private string? _fcmToken;
    private string? _fcmClient;
    private readonly L L;
    private Session? _ses;
    private readonly DotNetObjectReference<AuthService<TApi>> _dnObj;

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

    public AuthService(TApi api, L l, IJSRuntime js)
    {
        _api = api;
        _js = js;
        L = l;
        _dnObj = DotNetObjectReference.Create(this);
    }

    public void RegisterRefreshUi(Action<ISession> a)
    {
        _refreshUI = a;
    }

    public async Task<ISession> GetSession()
    {
        if (Session == null)
        {
            Session = await _api.Auth.GetSession();
            // if session was null this is the first time we're getting it
            // so if fcm is supposed to be enabled then trigger getting the token
            if (Session.FcmEnabled)
            {
                await FcmEnabled(true);
            }
        }

        return Session;
    }

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

    public async Task FcmEnabled(bool enabled)
    {
        var ses = await GetSession();
        if (ses.IsAuthed && enabled)
        {
            // if enabling go through js to ensure notification permission
            // has been granted
            await _js.InvokeVoidAsync("fcmGetToken", _dnObj);
        }
        else if (ses.FcmEnabled)
        {
            // if turning off, just turn off
            Session = await _api.Auth.FcmEnabled(new(false));
        }
    }

    public async Task FcmRegister(List<string> topic)
    {
        var ses = await GetSession();
        if (
            ses.IsAuthed
            && ses.FcmEnabled
            && !_fcmClient.IsNullOrEmpty()
            && !_fcmToken.IsNullOrEmpty()
        )
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

    [JSInvokable]
    public async Task FcmTokenCallback(string? token)
    {
        _fcmToken = token;
        var ses = await GetSession();
        var setEnabled = !token.IsNullOrEmpty();
        if (setEnabled != ses.FcmEnabled)
        {
            Session = await _api.Auth.FcmEnabled(new(setEnabled));
        }
    }

    [JSInvokable]
    public async Task FcmNotificationPermissionRemoved()
    {
        var ses = await GetSession();
        if (ses.FcmEnabled)
        {
            // user has switched off their notifications on this site
            // so disable fcm
            Session = await _api.Auth.FcmEnabled(new(false));
        }
    }

    public void Dispose()
    {
        _dnObj.Dispose();
    }
}
