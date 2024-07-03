using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record VerifyEmail([property: Key(0)] string Email, [property: Key(1)] string Code);
