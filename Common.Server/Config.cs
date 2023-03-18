using Amazon;
using Common.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Server;

public enum Env
{
    LCL,
    DEV,
    STG,
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

    public RegionEndpoint GetRegionEndpoint()
    {
        var re = RegionEndpoint.GetBySystemName(Region);
        if (re == null)
        {
            throw new InvalidSetupException(
                $"couldn't find aws region endpoint with system name: {Region}"
            );
        }
        return re;
    }
}

public static class Config
{
    private static readonly Raw _raw;

    public static Env Env
    {
        get => _raw.Env;
    }

    public static ServerConfig Server
    {
        get => _raw.Server;
    }
    public static DbConfig Db
    {
        get => _raw.Db;
    }
    public static SessionConfig Session
    {
        get => _raw.Session;
    }
    public static EmailConfig Email
    {
        get => _raw.Email;
    }

    static Config()
    {
        _raw = JsonConvert
            .DeserializeObject<Raw>(
                File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), "config.json"))
            )
            .NotNull();
    }
}

internal record Raw
{
    [JsonConverter(typeof(StringEnumConverter))]
    public Env Env { get; init; } = Env.LCL;
    public ServerConfig Server { get; init; }
    public DbConfig Db { get; init; }
    public SessionConfig Session { get; init; }
    public EmailConfig Email { get; init; }
}
