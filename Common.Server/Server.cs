using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Common.Server;

public static class Server
{
    public static void Run<TDbCtx>(string[] args, S s)
        where TDbCtx : DbContext
    {
        var config = IConfig.Init();
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddApiServices<TDbCtx>(config, s);

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
            app.UseWebAssemblyDebugging();
        else
            app.UseHsts();
        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapFallbackToFile("index.html");
        app.Run(config.Server.Listen);
    }
}
