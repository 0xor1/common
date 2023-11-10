namespace Common.Server;

public record ClientConfig
{
    public bool DemoMode { get; init; }
    public string? RepoUrl { get; init; }
}
