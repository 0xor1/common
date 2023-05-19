using System.Text.Json;
using Common.Shared;
using Common.Shared.Auth;
using Microsoft.JSInterop;
using Radzen;

namespace Common.Client;

public class AuthService<TApi> : IAuthService, IDisposable
    where TApi : class, IApi
{
    private readonly SemaphoreSlim _sesSs = new(1, 1);
    private readonly SemaphoreSlim _fcmJsSs = new(1, 1);
    private bool _jsInitialized = false;
    private readonly IJSRuntime _js;
    private string? _fcmToken;
    private string? _fcmClient;
    private readonly L L;
    private readonly TApi _api;
    private Action<ISession>? _refreshUI;
    private string? _fcmTopicStr;
    private List<string>? _fcmTopic;
    private Action<string>? _fcmHandler;
    private readonly NotificationService _notificationService;
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

    public AuthService(TApi api, L l, NotificationService ns, IJSRuntime js)
    {
        _api = api;
        _js = js;
        L = l;
        _notificationService = ns;
        _dnObj = DotNetObjectReference.Create(this);
    }

    public void RegisterRefreshUi(Action<ISession> a) => _refreshUI = a;

    public async Task<ISession> GetSession()
    {
        if (Session != null)
            return Session;
        await _sesSs.WaitAsync();
        try
        {
            Session ??= await _api.Auth.GetSession();
        }
        finally
        {
            _sesSs.Release();
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

        Session = await _api.Auth.SignOut();
        _fcmClient = null;
        _fcmToken = null;
        return Session;
    }

    public async Task<ISession> Delete()
    {
        var ses = await GetSession();
        if (!ses.IsAuthed)
        {
            return ses;
        }

        Session = await _api.Auth.Delete();
        _fcmClient = null;
        _fcmToken = null;
        return Session;
    }

    public async Task<ISession> SetL10n(string lang, string dateFmt, string timeFmt) =>
        Session = await _api.Auth.SetL10n(new(lang, dateFmt, timeFmt));

    public async Task<ISession> FcmEnabled(bool enabled)
    {
        var ses = await GetSession();

        if (ses.IsAuthed && enabled)
        {
            // if enabling, always go through js to ensure notification permission
            // has been granted
            await _fcmJsSs.WaitAsync();
            try
            {
                if (!_jsInitialized)
                {
                    await _js.InvokeVoidAsync("fcmInit", _dnObj);
                    _jsInitialized = true;
                }
                await _js.InvokeVoidAsync("fcmGetToken");
            }
            finally
            {
                _fcmJsSs.Release();
            }
        }
        else if (ses.FcmEnabled)
        {
            // if turning off, just turn off
            Session = await _api.Auth.FcmEnabled(new(false));
        }
        return Session.NotNull();
    }

    public async Task FcmRegister(List<string> topic, Action<string> a)
    {
        var ses = await GetSession();
        _fcmTopicStr = Fcm.TopicString(topic);
        _fcmTopic = topic;
        _fcmHandler = a;
        if (ses.IsAuthed && ses.FcmEnabled && !_fcmToken.IsNullOrEmpty())
        {
            _fcmClient = (await _api.Auth.FcmRegister(new(topic, _fcmToken, _fcmClient))).Client;
        }
        else
        {
            // always call this to ensure js fcm is initialized, this will
            // result in a loop back call to this method once js has inited the fcm token
            await FcmEnabled(true);
        }
    }

    public async Task FcmUnregister()
    {
        if (!_fcmClient.IsNullOrEmpty())
        {
            _fcmTopicStr = null;
            _fcmTopic = null;
            _fcmHandler = null;
            await _api.Auth.FcmUnregister(new(_fcmClient));
        }
    }

    [JSInvokable]
    public async Task FcmTokenCallback(string? token)
    {
        var ses = await GetSession();
        _fcmToken = token;
        if (!_fcmToken.IsNullOrEmpty() && !ses.FcmEnabled)
        {
            // if we have a token from the js then
            // permission is granted
            Session = await _api.Auth.FcmEnabled(new(true));
        }

        if (
            !_fcmToken.IsNullOrEmpty()
            && Session.NotNull().FcmEnabled
            && _fcmHandler != null
            && (_fcmTopic?.Any() ?? false)
        )
        {
            await FcmRegister(_fcmTopic, _fcmHandler);
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
            await FcmEnabled(false);
        }
    }

    [JSInvokable]
    public async Task FcmOnMessage(object? obj)
    {
        if (_fcmHandler == null)
        {
            return;
        }

        var notify = () =>
        {
            _notificationService.Notify(
                new()
                {
                    Severity = NotificationSeverity.Error,
                    Summary = L.S(S.AuthFcmMessageInvalid),
                }
            );
            throw new RpcException(L.S(S.AuthFcmMessageInvalid));
        };
        var js = (JsonElement?)obj;
        if (!js.HasValue)
        {
            notify();
        }

        var found = js.Value.TryGetProperty("data", out var payload);
        if (!found)
            notify();
        found = payload.TryGetProperty(Fcm.TypeName, out var type);
        if (!found)
            notify();
        found = payload.TryGetProperty(Fcm.ClientHeaderName, out var client);
        if (!found)
            notify();
        var typeStr = type.GetString();
        var clientStr = client.GetString();
        found = Enum.TryParse<FcmType>(typeStr, out var typeEnum);
        if (!found)
            notify();
        if (clientStr == _fcmClient)
        {
            Console.WriteLine(
                "fcm message received from action originating from this client, ignoring"
            );
            return;
        }
        if (typeEnum == FcmType.SignOut)
        {
            await SignOut();
            return;
        }

        if (typeEnum == FcmType.Disabled)
        {
            await FcmEnabled(false);
            return;
        }

        if (typeEnum == FcmType.Enabled)
        {
            await FcmEnabled(true);
            return;
        }

        // at this point the fcm type is data
        found = payload.TryGetProperty(Fcm.Topic, out var topic);
        if (!found)
            notify();
        if (topic.GetString() != _fcmTopicStr)
        {
            // we have received a notification for a topic we arent listening for
            // this can happen because of async/race condition type scenarios

            Console.WriteLine("fcm message received for topic Im not listening for, ignoring");
            return;
        }
        found = payload.TryGetProperty(Fcm.Data, out var data);
        if (!found)
            notify();
        var dataStr = data.GetString().NotNull();
        _fcmHandler(dataStr);
    }

    public void Dispose()
    {
        _dnObj.Dispose();
    }
}
