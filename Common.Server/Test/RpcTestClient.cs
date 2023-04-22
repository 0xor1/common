using System.Net;
using System.Reflection;
using Common.Server.Auth;
using Common.Shared;
using Common.Server;
using Common.Shared.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server.Test;

public class RpcTestRig<TDbCtx> : IAsyncDisposable
    where TDbCtx : DbContext, IAuthDb
{
    private readonly string Id = Shared.Id.New();
    private readonly IServiceProvider _services;
    private readonly IConfig _config;
    private readonly S _s;
    private readonly IReadOnlyDictionary<string, IRpcEndpoint> _eps;

    public RpcTestRig(S s, IReadOnlyList<IRpcEndpoint> eps)
    {
        var ass = Assembly.GetCallingAssembly();
        var configName = ass.GetManifestResourceNames().Single(x => x.EndsWith("config.json"));
        var configStream = ass.GetManifestResourceStream(configName).NotNull();
        var streamReader = new StreamReader(configStream);
        var configStr = streamReader.ReadToEnd();
        _config = Config.FromJson(configStr);
        _s = s;
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

    private TDbCtx GetDb()
    {
        return _services.GetRequiredService<TDbCtx>();
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
        return (rpcCtx.Session.NotNull(), rpcCtx.Res.NotNull());
    }

    private IRpcClient NewClient() => new RpcTestClient(Exe);

    private List<string> _registeredEmails = new();

    public async Task<T> NewApi<T>(Func<IRpcClient, T> cnstr, string? name = null)
        where T : IAuthApi
    {
        var api = cnstr(NewClient());
        if (!name.IsNullOrWhiteSpace())
        {
            var email = $"0xor1.common.server.test.{name}@{Id}.{name}";
            var pwd = "asdASD123@";
            await api.Register(new(email, "asdASD123@"));
            await using var db = GetDb();
            var code = db.Auths.Single(x => x.Email == email).VerifyEmailCode;
            await api.VerifyEmail(new(email, code));
            await api.SignIn(new(email, pwd, false));
            _registeredEmails.Add(email);
        }
        return api;
    }

    public async ValueTask DisposeAsync()
    {
        await using var db = GetDb();
        await db.Auths.Where(x => _registeredEmails.Contains(x.Email)).ExecuteDeleteAsync();
    }
}

public class RpcTestClient : IRpcClient
{
    private Session? _session;
    private Func<string, Session?, object, Task<(Session, object)>> _exe;

    public RpcTestClient(Func<string, Session?, object, Task<(Session, object)>> exe)
    {
        _exe = exe;
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
    public Exception? Exception { get; set; }

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

    public Task HandleException(Exception ex, string path)
    {
        Exception = ex;
        return Task.CompletedTask;
    }
}
