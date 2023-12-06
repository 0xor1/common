using Common.Shared;
using Common.Shared.Auth;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server.Test;

public record RpcTestCtx : IRpcCtxInternal
{
    private readonly IServiceProvider _services;
    private readonly IFeatureCollection _features;
    private readonly S _s;
    public Session Session { get; set; }
    public object Arg { get; set; }
    public object? Res { get; set; }
    public RpcTestException? Exception { get; set; }

    public Dictionary<string, string> Headers { get; set; }

    public RpcTestCtx(
        IServiceProvider services,
        IFeatureCollection features,
        ISession? session,
        S s,
        Dictionary<string, string> headers,
        object arg,
        CancellationToken ctkn
    )
    {
        _services = services;
        _features = features;
        _s = s;
        Session = (Session)(session ?? ClearSession());
        Headers = headers;
        Arg = arg;
        Ctkn = ctkn;
    }

    public CancellationToken Ctkn { get; init; }

    public T Get<T>()
        where T : notnull => _services.GetRequiredService<T>();

    public T GetFeature<T>()
        where T : notnull => _features.GetRequiredFeature<T>();

    public ISession GetSession() => Session;

    public ISession CreateSession(
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        DateFmt dateFmt,
        string timeFmt,
        string dateSeparator,
        string thousandsSeparator,
        string decimalSeparator,
        bool fcmEnabled
    )
    {
        Session = new Session()
        {
            Id = userId,
            StartedOn = DateTime.UtcNow,
            IsAuthed = isAuthed,
            RememberMe = rememberMe,
            Lang = lang,
            DateFmt = dateFmt,
            TimeFmt = timeFmt,
            DateSeparator = dateSeparator,
            ThousandsSeparator = thousandsSeparator,
            DecimalSeparator = decimalSeparator,
            FcmEnabled = fcmEnabled
        };
        return Session;
    }

    public ISession ClearSession()
    {
        Session = new Session()
        {
            Id = Id.New(),
            IsAuthed = false,
            RememberMe = false,
            Lang = _s.DefaultLang,
            DateFmt = _s.DefaultDateFmt,
            TimeFmt = _s.DefaultTimeFmt,
            DateSeparator = _s.DefaultDateSeparator,
            ThousandsSeparator = _s.DefaultThousandsSeparator,
            DecimalSeparator = _s.DefaultDecimalSeparator
        };
        return Session;
    }

    public string? GetHeader(string name) => Headers.ContainsKey(name) ? Headers[name] : null;

    public Task<T> GetArg<T>()
        where T : class
    {
        return ((T)Arg).AsTask();
    }

    public Task WriteResp<T>(T val)
        where T : class
    {
        Res = val;
        if (val is FcmRegisterRes regRes)
        {
            Headers.Remove(Fcm.ClientHeaderName);
            Headers.Add(Fcm.ClientHeaderName, regRes.Client);
        }
        return Task.CompletedTask;
    }

    public Task HandleException(Exception ex, string message, int code)
    {
        Exception = new RpcTestException(ex, new RpcException(message, code));
        return Task.CompletedTask;
    }
}
