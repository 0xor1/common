namespace Common.Server;

public interface IConfig
{
    public Env Env { get; }
    public ClientConfig Client { get; }
    public ServerConfig Server { get; }
    public DbConfig Db { get; }
    public SessionConfig Session { get; }
    public EmailConfig Email { get; }
    public StoreConfig Store { get; }
    public FcmConfig Fcm { get; }
}
