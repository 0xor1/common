using System.Net;
using System.Reflection;
using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server.Test;

public class RpcTestException : Exception
{
    public Exception Original { get; }
    public RpcException Rpc { get; }

    public RpcTestException(Exception original, RpcException rpc)
        : base(
            $"Original: {original.Message}\nRpc: {rpc.Message}\nOriginal Stacktrace: {original.StackTrace}\nRpc Stacktrace: {rpc.StackTrace}"
        )
    {
        Original = original;
        Rpc = rpc;
    }
};

public class RpcTestRig<TDbCtx, TApi> : IDisposable
    where TDbCtx : DbContext, IAuthDb
    where TApi : IApi
{
    private readonly string Id = Shared.Id.New();
    private readonly IServiceProvider _services;
    private readonly IConfig _config;
    private readonly S _s;
    private readonly Func<IRpcClient, TApi> _apiFactory;
    private readonly IReadOnlyDictionary<string, IRpcEndpoint> _eps;

    public RpcTestRig(S s, IReadOnlyList<IRpcEndpoint> eps, Func<IRpcClient, TApi> apiFactory)
    {
        var ass = Assembly.GetCallingAssembly();
        var configName = ass.GetManifestResourceNames().Single(x => x.EndsWith("config.json"));
        var configStream = ass.GetManifestResourceStream(configName).NotNull();
        var streamReader = new StreamReader(configStream);
        var configStr = streamReader.ReadToEnd();
        _config = Config.FromJson(configStr);
        _s = s;
        _apiFactory = apiFactory;
        var services = new ServiceCollection();
        services.AddApiServices<TDbCtx>(_config, s);
        _services = services.BuildServiceProvider();
        var dupedPaths = eps.Select(x => x.Path).GetDuplicates().ToList();
        Throw.SetupIf(
            dupedPaths.Any(),
            $"Some rpc endpoints have duplicate paths {string.Join(",", dupedPaths)}"
        );
        _eps = eps.ToDictionary(x => x.Path).AsReadOnly();
    }

    public T RunDb<T>(Func<TDbCtx, T> fn)
    {
        using var scope = _services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<TDbCtx>();
        return fn(db);
    }

    private async Task<(Session, object)> Exe(string path, Session? session, object arg)
    {
        using var scope = _services.CreateScope();
        var rpcCtx = new RpcTestCtx(scope.ServiceProvider, session, _s, arg);
        rpcCtx.ErrorIf(
            !_eps.TryGetValue(path, out var ep),
            S.RpcUnknownEndpoint,
            null,
            HttpStatusCode.NotFound
        );
        await ep.NotNull().Execute(rpcCtx);
        if (rpcCtx.Exception != null)
        {
            throw rpcCtx.Exception;
        }
        return (rpcCtx.Session.NotNull(), rpcCtx.Res.NotNull());
    }

    private IRpcClient NewClient(Session? session = null) => new RpcTestClient(Exe, session);

    private HashSet<string> _registeredEmails = new();

    public async Task<(TApi, string Email, string Pwd)> NewApi(string? name = null)
    {
        var api = _apiFactory(NewClient());
        var email = "";
        var pwd = "";
        if (!name.IsNullOrWhiteSpace())
        {
            email = $"{name}@{Id}.{name}".ToLowerInvariant();
            pwd = "asdASD123@";
            await api.Auth.Register(new(email, "asdASD123@"));
            var code = RunDb((db) => db.Auths.Single(x => x.Email == email).VerifyEmailCode);
            await api.Auth.VerifyEmail(new(email, code));
            await api.Auth.SignIn(new(email, pwd, false));
            _registeredEmails.Add(email);
        }
        return (api, email, pwd);
    }

    public void Dispose()
    {
        var ids = RunDb<List<string>>(db =>
        {
            return db.Auths
                .Where(x => _registeredEmails.Contains(x.Email))
                .Select(x => x.Id)
                .ToList();
        });

        foreach (var id in ids)
        {
            var t = _apiFactory(
                NewClient(
                    new Session()
                    {
                        Id = id,
                        IsAuthed = true,
                        Lang = _s.DefaultLang,
                        DateFmt = _s.DefaultDateFmt,
                        TimeFmt = _s.DefaultTimeFmt
                    }
                )
            ).Auth.Delete();
            t.Wait();
            if (t.Exception != null)
            {
                throw t.Exception;
            }
        }
    }
}

public class RpcTestClient : IRpcClient
{
    private Session? _session;
    private Func<string, Session?, object, Task<(Session, object)>> _exe;

    public RpcTestClient(
        Func<string, Session?, object, Task<(Session, object)>> exe,
        Session? session = null
    )
    {
        _exe = exe;
        _session = session;
    }

    public async Task<TRes> Do<TArg, TRes>(Rpc<TArg, TRes> rpc, TArg arg)
        where TArg : class
        where TRes : class
    {
        (_session, var res) = await _exe(rpc.Path, _session, arg);
        return (TRes)res;
    }
}

public record RpcTestCtx : IRpcCtxInternal
{
    private readonly IServiceProvider _services;
    private readonly S _s;
    public Session Session { get; set; }
    public object Arg { get; set; }
    public object? Res { get; set; }
    public RpcTestException? Exception { get; set; }

    public RpcTestCtx(IServiceProvider services, Session? session, S s, object arg)
    {
        _services = services;
        _s = s;
        Session = session ?? ClearSession();
        Arg = arg;
    }

    public T Get<T>()
        where T : notnull => _services.GetRequiredService<T>();

    public Session GetSession() => Session;

    public Session CreateSession(
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        string dateFmt,
        string timeFmt
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
            TimeFmt = timeFmt
        };
        return Session;
    }

    public Session ClearSession()
    {
        Session = new()
        {
            Id = Id.New(),
            IsAuthed = false,
            RememberMe = false,
            Lang = _s.DefaultLang,
            DateFmt = _s.DefaultDateFmt,
            TimeFmt = _s.DefaultTimeFmt
        };
        return Session;
    }

    public Task<T> GetArg<T>()
        where T : class
    {
        return ((T)Arg).AsTask();
    }

    public Task WriteResp<T>(T val)
        where T : class
    {
        Res = val;
        return Task.CompletedTask;
    }

    public Task HandleException(Exception ex, string message, int code)
    {
        Exception = new RpcTestException(ex, new RpcException(message, code));
        return Task.CompletedTask;
    }
}
