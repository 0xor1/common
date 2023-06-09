using System.ComponentModel;
using System.Runtime.Serialization;
using Amazon;
using Common.Shared;
using Newtonsoft.Json;

namespace Common.Server;

public record DevServerConfig
{
    public string Listen { get; init; }
    public string RpcHost { get; init; }
}

public record DevConfig : IDevConfig
{
    public DevServerConfig DevServer { get; init; }

    public static DevConfig FromJson(string s) => Json.To<DevConfig>(s);
}

public interface IDevConfig
{
    public DevServerConfig DevServer { get; }
}
