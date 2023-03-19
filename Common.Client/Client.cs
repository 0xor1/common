using Common.Shared.I18n;
using Grpc.Core;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Client;

public static class Client
{
    public static async Task Run<TApp, TApi, TAuthService>(string[] args, S s, Func<CallInvoker, TApi> api) where TApp : IComponent where TApi : class where TAuthService : class, IAuthService
    {

        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<TApp>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddSingleton(s);
        builder.Services.AddSingleton(L.Init(s));
        builder.Services.AddSingleton<IAuthService, TAuthService>();
        builder.Services.AddSingleton<Radzen.NotificationService>();
        builder.Services.AddSingleton<ErrorInterceptor>();
        builder.Services.AddSingleton<TApi>(services =>
        {
            var httpClient = new HttpClient(
                new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler())
            );
            var baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
            var channel = GrpcChannel
                .ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient })
                .Intercept(services.GetRequiredService<ErrorInterceptor>());
            return api(channel);
        });
        await builder.Build().RunAsync();

    }
}