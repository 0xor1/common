using System.Net;
using System.Reflection;
using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server.Test;

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

    public RpcTestRig(
        S s,
        IReadOnlyList<IRpcEndpoint> eps,
        Func<IRpcClient, TApi> apiFactory,
        Action<IServiceCollection>? addServices = null,
        Func<IServiceProvider, Task>? initApp = null
    )
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
        services.AddApiServices<TDbCtx>(_config, s, addServices, initApp);
        _services = services.BuildServiceProvider();
        var dupedPaths = eps.Select(x => x.Path).GetDuplicates().ToList();
        Throw.SetupIf(
            dupedPaths.Any(),
            $"Some rpc endpoints have duplicate paths {string.Join(",", dupedPaths)}"
        );
        _eps = eps.ToDictionary(x => x.Path).AsReadOnly();
    }

    public T Get<T>()
        where T : notnull => _services.GetRequiredService<T>();

    public T RunDb<T>(Func<TDbCtx, T> fn)
    {
        using var scope = _services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<TDbCtx>();
        return fn(db);
    }

    private async Task<(Session, object)> Exe(
        string path,
        Session? session,
        Dictionary<string, string> headers,
        object arg,
        CancellationToken ctkn = default
    )
    {
        using var scope = _services.CreateScope();
        var features = new FeatureCollection();
        features[typeof(IHttpMaxRequestBodySizeFeature)] = new TestHttpMaxRequestBodySizeFeature();
        var rpcCtx = new RpcTestCtx(
            scope.ServiceProvider,
            features,
            session,
            _s,
            headers,
            arg,
            ctkn
        );
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
                        TimeFmt = _s.DefaultTimeFmt,
                        ThousandsSeparator = _s.DefaultThousandsSeparator,
                        DecimalSeparator = _s.DefaultDecimalSeparator
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
