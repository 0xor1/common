using Cocona;
using Common.Cli;
using Microsoft.Extensions.DependencyInjection;

var builder = CoconaApp.CreateBuilder();
builder.Services.AddLogging();
var app = builder.Build();
app.AddCommands<Api>();
app.AddCommands<I18n>();
app.AddCommands<Dnsk>();
await app.RunAsync();