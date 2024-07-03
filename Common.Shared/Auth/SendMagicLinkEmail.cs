using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record SendMagicLinkEmail(
    [property: Key(0)] string Email,
    [property: Key(1)] bool RememberMe
);
