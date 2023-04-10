using Common.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace Common.Client;

public static class Client
{
    public static async Task Run<TApp, TAuthService>(string[] args, S s)
        where TApp : IComponent where TAuthService : class, IAuthService
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<TApp>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddSingleton(s);
        builder.Services.AddSingleton(L.Init(s));
        builder.Services.AddSingleton<IAuthService, TAuthService>();
        builder.Services.AddSingleton<NotificationService>();
        await builder.Build().RunAsync();
    }
}