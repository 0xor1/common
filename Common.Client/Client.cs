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
        var api = apiFactory(rpcClient);
        builder.Services.AddSingleton(httpClient);
        builder.Services.AddSingleton<IRpcClient>(rpcClient);
        builder.Services.AddSingleton(s);
        builder.Services.AddSingleton<L>(l);
        builder.Services.AddSingleton(api);
        builder.Services.AddSingleton<IApi>(api);
        builder.Services.AddSingleton<IAuthService, AuthService<TApi>>();
        builder.Services.AddSingleton(ns);
        builder.Services.AddSingleton<DialogService>();
        builder.Services.AddSingleton<TooltipService>();
        builder.Services.AddSingleton<ContextMenuService>();
        addServices?.Invoke(builder.Services);
        await builder.Build().RunAsync();
    }
}
