using System.ComponentModel;
using System.Runtime.Serialization;
using Amazon;
using Common.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Server;

[JsonConverter(typeof(StringEnumConverter))]
public enum Env
{
    [EnumMember(Value = "lcl")]
    [Description("lcl")]
    LCL,

    [EnumMember(Value = "dev")]
    [Description("dev")]
    DEV,

    [EnumMember(Value = "stg")]
    [Description("stg")]
    STG,

    [EnumMember(Value = "pro")]
    [Description("pro")]
    PRO
}

public record ServerConfig
{
    public string Listen { get; init; }
}

public record DbConfig
{
    public string Connection { get; init; }
}

public record SessionConfig
{
    public IReadOnlyList<string> SignatureKeys { get; init; }
}

public record EmailConfig
{
    public string Region { get; init; }
    public string Key { get; init; }
    public string Secret { get; init; }
    public string NoReplyAddress { get; init; }

    [JsonIgnore]
    public RegionEndpoint RegionEndpoint => Region.GetRegionEndpoint();
}

public record StoreConfig
{
    public string Region { get; init; }
    public string Key { get; init; }
    public string Secret { get; init; }

    [JsonIgnore]
    public RegionEndpoint RegionEndpoint => Region.GetRegionEndpoint();
}

internal record Config : IConfig
{
    public Env Env { get; init; } = Env.LCL;
    public ServerConfig Server { get; init; }
    public DbConfig Db { get; init; }
    public SessionConfig Session { get; init; }
    public EmailConfig Email { get; init; }
    public StoreConfig Store { get; init; }
}

public static class AwsStringExts
{
    public static RegionEndpoint GetRegionEndpoint(this string region)
    {
        var re = RegionEndpoint.GetBySystemName(region);
        if (re == null)
            throw new InvalidSetupException(
                $"couldn't find aws region endpoint with system name: {region}"
            );
        return re;
    }
}

public interface IConfig
{
    private static IConfig? _inst;
    public Env Env { get; }
    public ServerConfig Server { get; }
    public DbConfig Db { get; }
    public SessionConfig Session { get; }
    public EmailConfig Email { get; }
    public StoreConfig Store { get; }

    public static IConfig Init()
    {
        return _inst ??= JsonConvert
            .DeserializeObject<Config>(
                File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), "config.json"))
            )
            .NotNull();
    }
}
