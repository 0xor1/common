namespace Common.Shared.Test;

public class IEnumerableExtsTests
{
    [Fact]
    public void Join_Success()
    {
        Assert.Equal("1, 2, 3", new List<string> { "1", "2", "3" }.Join());
        Assert.Equal("a, b, c, d", new List<Key> { new("a"), new("b"), new("c"), new("d") }.Join());
        Assert.Equal("1,2,3", new List<string> { "1", "2", "3" }.Join(","));
        Assert.Equal("a,b,c,d", new List<Key> { new("a"), new("b"), new("c"), new("d") }.Join(","));
    }

    [Fact]
    public void GetDuplicates_WorksWithStrings()
    {
        Assert.Empty(new List<string> { "one", "two", "three" }.GetDuplicates());
        Assert.Equal(
            "one",
            new List<string> { "one", "two", "three", "one" }.GetDuplicates().Single()
        );
    }

    [Fact]
    public void GetDuplicates_WorksWithKeys()
    {
        Assert.Empty(new List<Key> { new("one"), new("two"), new("three") }.GetDuplicates());
        Assert.Equal(
            new Key("one"),
            new List<Key> { new("one"), new("two"), new("three"), new("one") }
                .GetDuplicates()
                .Single()
        );
    }

    [Fact]
    public void ForEach_Success()
    {
        var i = 0;
        IEnumerable<Key> things = new List<Key> { new("one"), new("two"), new("three") };
        things.ForEach(x => i++);
        Assert.Equal(3, i);
    }
}
