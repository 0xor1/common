using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record SignIn(
    [property: Key(0)] string Email,
    [property: Key(1)] string Pwd,
    [property: Key(2)] bool RememberMe
);
