using Cocona;
using Common.Cmds;

var builder = CoconaApp.CreateBuilder();
var app = builder.Build();
app.AddCommands<I18n>();
await app.RunAsync();