using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record FcmUnregister([property: Key(0)] string Client);
