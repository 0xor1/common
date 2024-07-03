using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record SendResetPwdEmail([property: Key(0)] string Email);
