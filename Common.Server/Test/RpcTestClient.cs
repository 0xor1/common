using System.Net;
using Common.Server.Auth;
using Common.Shared;
using Common.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server.Test;

public class RpcTestRig<TDbCtx>
    where TDbCtx : DbContext, IAuthDb
{
    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly IConfig _config;
    private readonly S _s;
    private readonly IReadOnlyDictionary<string, IRpcEndpoint> _eps;

    public RpcTestRig(IConfig config, S s, IReadOnlyList<IRpcEndpoint> eps)
    {
        _config = config;
        _s = s;
        _services.AddApiServices<TDbCtx>(config, s);
        var dupedPaths = eps.Select(x => x.Path).GetDuplicates().ToList();
        Throw.SetupIf(
            dupedPaths.Any(),
            $"Some rpc endpoints have duplicate paths {string.Join(",", dupedPaths)}"
        );
        _eps = eps.ToDictionary(x => x.Path).AsReadOnly();
    }

    private async Task<(Session, object)> Exe(string path, Session session, object arg)
    {
        var rpcCtx = new RpcTestCtx(
            _services.BuildServiceProvider().CreateScope().ServiceProvider,
            _s,
            arg
        );
        rpcCtx.ErrorIf(
            !_eps.TryGetValue(path, out var ep),
            S.RpcUnknownEndpoint,
            null,
            HttpStatusCode.NotFound
        );
        await ep.NotNull().Execute(rpcCtx);
        return (rpcCtx.Session.NotNull(), rpcCtx.Res.NotNull());
    }

    public IRpcClient NewClient() => new RpcTestClient(Exe);
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

    public RpcTestCtx(IServiceProvider services, S s, object arg)
    {
        _services = services;
        _s = s;
        Session = ClearSession();
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
