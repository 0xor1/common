using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record FcmRegister(
    [property: Key(0)] IReadOnlyList<string> Topic,
    [property: Key(1)] string Token,
    [property: Key(2)] string? Client
);
