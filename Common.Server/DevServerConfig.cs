namespace Common.Server;

public record DevServerConfig
{
    public string Listen { get; init; }
    public string RpcHost { get; init; }
}
