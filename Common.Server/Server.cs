using Common.Server.Auth;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Server;

public static class Server
{
    public static void Run<TDbCtx, THost>(
        string[] args,
        S s,
        IReadOnlyList<IRpcEndpoint> eps,
        Action<IServiceCollection>? addServies = null,
        Func<IServiceProvider, Task>? initApp = null
    )
        where TDbCtx : DbContext, IAuthDb
    {
        var config = Config.FromJson(
            File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), "config.json"))
        );
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
        builder.Services.AddApiServices<TDbCtx>(config, s, addServies, initApp);

        var app = builder.Build();
        if (config.Env == Env.Lcl)
            app.UseWebAssemblyDebugging();
        else
            app.UseHsts();
        if (config.Server.UseHttpsRedirection)
            app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRpcEndpoints(eps);
        app.MapRazorComponents<THost>().AddInteractiveWebAssemblyRenderMode();
        app.Run(config.Server.Listen);
    }
}
