using Common.Shared;
using Common.Shared.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using CS = Common.Shared.I18n.S;

namespace Common.Client;

public static class Client
{
    public static async Task Run<TApp, TApi>(
        string[] args,
        S s,
        Func<IRpcClient, TApi> apiFactory,
        Action<IServiceCollection>? addServices = null,
        bool enableRequestStreaming = false
    )
        where TApp : IComponent
        where TApi : class, IApi
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<TApp>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var httpClient = new HttpClient();
        var l = new Localizer(s);
        var ns = new NotificationService();
        var rpcClient = new RpcHttpClient(
            builder.HostEnvironment.BaseAddress,
            httpClient,
            (message) =>
                ns.Notify(NotificationSeverity.Error, l.S(CS.ApiError), message, duration: 6000D),
            enableRequestStreaming
        );
        builder.Services.AddSingleton(httpClient);
        Setup(builder.Services, l, s, ns, apiFactory(rpcClient), addServices);
        await builder.Build().RunAsync();
    }

    public static void Setup<TApi>(
        IServiceCollection services,
        L l,
        S s,
        NotificationService ns,
        TApi api,
        Action<IServiceCollection>? addServices = null
    )
        where TApi : class, IApi
    {
        services.AddSingleton(s);
        services.AddSingleton(l);
        services.AddSingleton(api);
        services.AddSingleton<IApi>(api);
        services.AddSingleton<IAuthService, AuthService<TApi>>();
        services.AddSingleton(ns);
        services.AddSingleton<DialogService>();
        services.AddSingleton<TooltipService>();
        services.AddSingleton<ContextMenuService>();
        addServices?.Invoke(services);
    }
}
