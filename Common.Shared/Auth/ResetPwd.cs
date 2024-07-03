using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record ResetPwd(
    [property: Key(0)] string Email,
    [property: Key(1)] string Code,
    [property: Key(2)] string NewPwd
);
