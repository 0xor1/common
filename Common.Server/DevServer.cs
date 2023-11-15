using Common.Server.Auth;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Server;

public static class DevServer
{
    public static void Run(string[] args)
    {
        var config = DevConfig.FromJson(
            File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), "config.json"))
        );
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddHttpClient();

        var app = builder.Build();
        app.UseWebAssemblyDebugging();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRpcHost(config.DevServer.RpcHost);
        app.MapFallbackToFile("index.html");
        app.Run(config.DevServer.Listen);
    }
}
