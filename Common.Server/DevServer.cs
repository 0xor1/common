using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server;

public static class DevServer
{
    public static void Run(string[] args)
    {
        var config = DevConfig.FromJson(
            File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), "config.json"))
        );
        var builder = WebApplication.CreateBuilder(args);
        builder
            .Services
            .AddHttpClient("dev_server")
            .ConfigurePrimaryHttpMessageHandler(
                _ =>
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (
                            sender,
                            cert,
                            chain,
                            sslPolicyErrors
                        ) =>
                        {
                            // I dont know why this is needed, when pointing
                            // at my current demo sites:
                            // https://dnsk.dans-demos.com
                            // https://oak.dans-demos.com
                            // they have valid ssl certs that all major browsers are
                            // happy to trust, but this http client doesnt trust them
                            // for some reason.
                            return true;
                        }
                    }
            );

        var app = builder.Build();
        app.UseWebAssemblyDebugging();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRpcHost(config.DevServer.RpcHost);
        app.MapFallbackToFile("index.html");
        app.Run(config.DevServer.Listen);
    }
}
