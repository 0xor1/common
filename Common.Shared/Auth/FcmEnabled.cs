using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record FcmEnabled([property: Key(0)] bool Val);
