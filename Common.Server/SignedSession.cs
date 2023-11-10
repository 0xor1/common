using MessagePack;

namespace Common.Server;

[MessagePackObject]
public record SignedSession
{
    [Key(0)]
    public byte[] Session { get; init; }

    [Key(1)]
    public byte[] Signature { get; init; }
}
