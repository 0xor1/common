using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record Config([property: Key(0)] bool DemoMode, [property: Key(1)] string? RepoUrl);
