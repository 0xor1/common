using Common.Shared;

namespace Common.Server;

public record Config : IConfig
{
    public Env Env { get; init; } = Env.Lcl;

    public ClientConfig Client { get; init; }
    public ServerConfig Server { get; init; }
    public DbConfig Db { get; init; }
    public SessionConfig Session { get; init; }
    public EmailConfig Email { get; init; }
    public StoreConfig Store { get; init; }
    public FcmConfig Fcm { get; init; }

    public static Config FromJson(string s) => Json.To<Config>(s);
}
