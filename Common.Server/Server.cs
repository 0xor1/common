using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Common.Server;

public static class Server
{
    public static void Run<TDbCtx, TApiService>(string[] args, string unexpectedError)
        where TDbCtx : DbContext
        where TApiService : class
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddApiServices<TDbCtx>(unexpectedError);

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
        app.MapGrpcService<TApiService>();
        app.MapFallbackToFile("index.html");
        app.Run(Config.Server.Listen);
    }
}
