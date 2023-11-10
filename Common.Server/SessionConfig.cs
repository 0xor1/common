namespace Common.Server;

public record SessionConfig
{
    public IReadOnlyList<string> SignatureKeys { get; init; }
}
