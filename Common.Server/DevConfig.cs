using System.ComponentModel;
using System.Runtime.Serialization;
using Amazon;
using Common.Shared;
using Newtonsoft.Json;

namespace Common.Server;

public record DevConfig : IDevConfig
{
    public DevServerConfig DevServer { get; init; }

    public static DevConfig FromJson(string s) => Json.To<DevConfig>(s);
}
