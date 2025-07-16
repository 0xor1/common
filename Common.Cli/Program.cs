using ConsoleAppFramework;
using Common.Cli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;

var app = ConsoleApp.Create();
app.ConfigureServices(services =>
{
    services.AddLogging(b =>
    {
        b.ClearProviders();
        b.AddZLoggerConsole(x => x.LogToStandardErrorThreshold = LogLevel.Error);
        b.SetMinimumLevel(LogLevel.Information);
    });
});
app.Add<Api>();
app.Add<I18n>();
app.Add<Dnsk>();
await app.RunAsync(args);