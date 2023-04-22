namespace Common.Shared.Test;

public class RpcTests
{
    [Fact]
    public void SerializeDeserialize_TrimsStrings()
    {
        var res = RpcHttp.Deserialize<Test>(RpcHttp.Serialize(new Test("  yolo  ")));
        Assert.Equal("yolo", res.Value);
    }

    public record Test(string? Value);
}