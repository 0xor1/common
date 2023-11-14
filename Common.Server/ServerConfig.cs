namespace Common.Server;

public record ServerConfig
{
    public string Listen { get; init; }
    public bool UseHttpsRedirection { get; init; }
}
