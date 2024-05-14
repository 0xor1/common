namespace Common.Shared.Test;

public class DictionaryExtTests
{
    [Fact]
    public void Try_Struct_Success()
    {
        var dic = new Dictionary<string, int>();
        var res = dic.Try("a");
        Assert.False(res.Got);
        Assert.Null(res.Val);
        dic.Add("a", 1);
        res = dic.Try("a");
        Assert.True(res.Got);
        Assert.Equal(1, res.Val);
    }

    [Fact]
    public void Try_Class_Success()
    {
        var dic = new Dictionary<string, string>();
        var res = dic.TryRef("a");
        Assert.False(res.Got);
        Assert.Null(res.Val);
        dic.Add("a", "b");
        res = dic.TryRef("a");
        Assert.True(res.Got);
        Assert.Equal("b", res.Val);
    }
}
