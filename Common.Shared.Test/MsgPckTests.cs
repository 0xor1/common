using MessagePack;

namespace Common.Shared.Test;

public class MsgPckTests
{
    [Fact]
    public void NSet_and_Maybe_MsgPck_Success()
    {
        var a = new Maybe<NSet<AThing>>(new NSet<AThing>(new AThing("yolo", 1)));
        var bs = MsgPck.From(a);
        var b = MsgPck.To<Maybe<NSet<AThing>>>(bs);
        Assert.Equal(a, b);
    }
}

[MessagePackObject]
public record AThing
{
    [SerializationConstructor]
    public AThing(string s, int i)
    {
        String = s;
        Int = i;
    }

    [Key(0)]
    public string String { get; set; }

    [Key(1)]
    public int Int { get; set; }
}
