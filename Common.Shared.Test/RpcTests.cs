using MessagePack;

namespace Common.Shared.Test;

public class RpcTests
{
    [Fact]
    public void SerializeDeserialize_TrimsStrings()
    {
        var res = RpcHttp.Deserialize<Test>(RpcHttp.Serialize(new Test("  yolo  ")));
        Assert.Equal("yolo", res.Value);
    }

    [MessagePackObject(true)]
    public record Test([property: Key(0)] string? Value);
}
