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
        RpcBase.Init(builder.HostEnvironment.BaseAddress, client);

        builder.Services.AddSingleton(s);
        builder.Services.AddSingleton(L.Init(s));
        builder.Services.AddSingleton(client);
        builder.Services.AddSingleton(api);
        builder.Services.AddSingleton<IAuthService, AuthService<TApi>>();
        builder.Services.AddSingleton<NotificationService>();
        await builder.Build().RunAsync();
    }
}