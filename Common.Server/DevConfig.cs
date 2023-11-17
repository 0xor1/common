using Common.Shared;

namespace Common.Server;

public record DevConfig : IDevConfig
{
    public DevServerConfig DevServer { get; init; }

    public static DevConfig FromJson(string s) => Json.To<DevConfig>(s);
}
