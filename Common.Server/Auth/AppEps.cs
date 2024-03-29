using Common.Shared;
using Common.Shared.Auth;

namespace Common.Server.Auth;

public static class AppEps
{
    public static IReadOnlyList<IRpcEndpoint> Eps { get; } =
        new List<IRpcEndpoint>()
        {
            new RpcEndpoint<Nothing, Shared.Auth.Config>(
                AppRpcs.GetConfig,
                async (ctx, _) =>
                {
                    await Task.CompletedTask;
                    var conf = ctx.Get<IConfig>();
                    return new(conf.Client.DemoMode, conf.Client.RepoUrl);
                }
            ),
        };
}
