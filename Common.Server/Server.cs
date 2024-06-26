using Common.Server.Auth;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server;

public static class Server
{
    public static void Run<TDbCtx>(
        string[] args,
        S s,
        IReadOnlyList<IEp> eps,
        Action<IServiceCollection>? addServies = null,
        Func<IServiceProvider, Task>? initApp = null
    )
        where TDbCtx : DbContext, IAuthDb
    {
        var config = Config.FromJson(
            File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), "config.json"))
        );
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddApiServices<TDbCtx>(config, s, addServies, initApp);

        var app = builder.Build();
        if (config.Env == Env.Lcl)
            app.UseWebAssemblyDebugging();
        else
            app.UseHsts();
        if (config.Server.UseHttpsRedirection)
            app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRpcEndpoints(eps);
        app.MapFallbackToFile("index.html");
        app.Run(config.Server.Listen);
    }
}
