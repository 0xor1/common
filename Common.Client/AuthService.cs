﻿using System.Text.Json;
using Common.Shared;
using Common.Shared.Auth;
using Microsoft.JSInterop;
using Radzen;
using S = Common.Shared.I18n.S;

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
    private Action<ISession>? _onSessionChanged;
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
            _ses = value.NotNull();
            L.Config(
                _ses.Lang,
                _ses.DateFmt,
                _ses.TimeFmt,
                _ses.DateSeparator,
                _ses.ThousandsSeparator,
                _ses.DecimalSeparator
            );
            _onSessionChanged?.Invoke(_ses);
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

    public void OnSessionChanged(Action<ISession> a) => _onSessionChanged = a;

    public async Task<ISession> GetSession(CancellationToken ctkn = default)
    {
        if (Session != null)
            return Session;
        await _sesSs.WaitAsync();
        try
        {
            Session ??= await _api.Auth.GetSession(ctkn);
        }
        finally
        {
            _sesSs.Release();
        }

        return Session;
    }

    public async Task Register(string email, string pwd, CancellationToken ctkn = default)
    {
        var ses = await GetSession(ctkn);
        Throw.OpIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
        await _api.Auth.Register(new(email, pwd), ctkn);
    }

    public async Task<ISession> SignIn(
        string email,
        string pwd,
        bool rememberMe,
        CancellationToken ctkn = default
    )
    {
        var ses = await GetSession(ctkn);
        Throw.OpIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
        return Session = await _api.Auth.SignIn(new(email, pwd, rememberMe), ctkn);
    }

    public async Task<ISession> MagicLinkSignIn(
        string email,
        string code,
        bool rememberMe,
        CancellationToken ctkn = default
    )
    {
        var ses = await GetSession(ctkn);
        Throw.OpIf(ses.IsAuthed, S.AuthAlreadyAuthenticated);
        return Session = await _api.Auth.MagicLinkSignIn(new(email, code, rememberMe), ctkn);
    }

    public async Task<ISession> SignOut(CancellationToken ctkn = default)
    {
        var ses = await GetSession(ctkn);
        if (!ses.IsAuthed)
        {
            return ses;
        }

        Session = await _api.Auth.SignOut(ctkn);
        _fcmClient = null;
        _fcmToken = null;
        return Session;
    }

    public async Task<ISession> Delete(CancellationToken ctkn = default)
    {
        var ses = await GetSession(ctkn);
        if (!ses.IsAuthed)
        {
            return ses;
        }

        Session = await _api.Auth.Delete(ctkn);
        _fcmClient = null;
        _fcmToken = null;
        return Session;
    }

    public async Task<ISession> SetL10n(
        string lang,
        DateFmt dateFmt,
        string timeFmt,
        string dateSeparator,
        string thousandsSeparator,
        string decimalSeparator,
        CancellationToken ctkn = default
    ) =>
        Session = await _api.Auth.SetL10n(
            new(lang, dateFmt, timeFmt, dateSeparator, thousandsSeparator, decimalSeparator),
            ctkn
        );

    public async Task<ISession> FcmEnabled(bool enabled, CancellationToken ctkn = default)
    {
        var ses = await GetSession(ctkn);

        if (ses.IsAuthed && enabled)
        {
            // if enabling, always go through js to ensure notification permission
            // has been granted
            await _fcmJsSs.WaitAsync(ctkn);
            try
            {
                if (!_jsInitialized)
                {
                    await _js.InvokeVoidAsync("fcmInit", ctkn, _dnObj);
                    _jsInitialized = true;
                }
                await _js.InvokeVoidAsync("fcmGetToken", ctkn);
            }
            finally
            {
                _fcmJsSs.Release();
            }
        }
        else if (ses.FcmEnabled)
        {
            // if turning off, just turn off
            Session = await _api.Auth.FcmEnabled(new(false), ctkn);
        }
        return Session.NotNull();
    }

    public async Task FcmRegister(
        List<string> topic,
        Action<string> a,
        CancellationToken ctkn = default
    )
    {
        var ses = await GetSession(ctkn);
        _fcmTopicStr = Fcm.TopicString(topic);
        _fcmTopic = topic;
        _fcmHandler = a;
        if (ses.IsAuthed && ses.FcmEnabled && !_fcmToken.IsNullOrEmpty())
        {
            _fcmClient = (
                await _api.Auth.FcmRegister(new(topic, _fcmToken, _fcmClient), ctkn)
            ).Client;
        }
        else
        {
            // always call this to ensure js fcm is initialized, this will
            // result in a loop back call to this method once js has inited the fcm token
            await FcmEnabled(true, ctkn);
        }
    }

    public async Task FcmUnregister(CancellationToken ctkn = default)
    {
        if (!_fcmClient.IsNullOrEmpty())
        {
            _fcmTopicStr = null;
            _fcmTopic = null;
            _fcmHandler = null;
            await _api.Auth.FcmUnregister(new(_fcmClient), ctkn);
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
            return;
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
