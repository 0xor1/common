using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record FcmRegisterRes([property: Key(0)] string Client);
