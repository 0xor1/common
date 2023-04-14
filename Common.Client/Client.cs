using Common.Shared;
using Common.Shared.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Common.Client;

public static class Client
{
    public static async Task Run<TApp, TApi>(string[] args, S s, TApi api)
        where TApp : IComponent where  TApi : class, IApi
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<TApp>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var client = new HttpClient();
        var l = L.Init(s);
        var ns = new NotificationService();
        RpcBase.Init(builder.HostEnvironment.BaseAddress, client, (strKey) => 
            ns.Notify(NotificationSeverity.Error, l.S(S.ApiError), l.S(strKey), duration: 10000D));

        builder.Services.AddSingleton(s);
        builder.Services.AddSingleton(l);
        builder.Services.AddSingleton(api);
        builder.Services.AddSingleton<IAuthService, AuthService<TApi>>();
        builder.Services.AddSingleton(ns);
        await builder.Build().RunAsync();
    }
}