using System.ComponentModel;
using System.Runtime.Serialization;
using Amazon;
using Common.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Server;

public enum Env
{
    [EnumMember(Value = "lcl")]
    [Description("lcl")]
    Lcl,

    [EnumMember(Value = "dev")]
    [Description("dev")]
    Dev,

    [EnumMember(Value = "stg")]
    [Description("stg")]
    Stg,

    [EnumMember(Value = "pro")]
    [Description("pro")]
    Pro
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
    public string Endpoint { get; init; }
    public string Key { get; init; }
    public string Secret { get; init; }

    [JsonIgnore]
    public RegionEndpoint RegionEndpoint => Region.GetRegionEndpoint();
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

public record Config : IConfig
{
    public Env Env { get; init; } = Env.Lcl;
    public ServerConfig Server { get; init; }
    public DbConfig Db { get; init; }
    public SessionConfig Session { get; init; }
    public EmailConfig Email { get; init; }
    public StoreConfig Store { get; init; }

    public static Config FromJson(string json) =>
        JsonConvert.DeserializeObject<Config>(json, SerializerSettings).NotNull();

    private static readonly JsonSerializerSettings SerializerSettings =
        new()
        {
            Formatting = Formatting.None,
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter(),
                new StrTrimConverter()
            }
        };
}

public interface IConfig
{
    public Env Env { get; }
    public ServerConfig Server { get; }
    public DbConfig Db { get; }
    public SessionConfig Session { get; }
    public EmailConfig Email { get; }
    public StoreConfig Store { get; }
}
