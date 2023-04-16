namespace Common.Shared.Test;

public class IEnumerableExtsTests
{
    [Fact]
    public void GetDuplicates_WorksWithStrings()
    {
        Assert.Empty(new List<string>
        {
            "one", "two", "three"
        }.GetDuplicates());
        Assert.Equal("one", new List<string>
        {
            "one", "two", "three", "one"
        }.GetDuplicates().Single());
    }

    [Fact]
    public void GetDuplicates_WorksWithKeys()
    {
        Assert.Empty(new List<Key>
        {
            new("one"), new("two"), new("three")
        }.GetDuplicates());
        Assert.Equal(new Key("one"), new List<Key>
        {
            new("one"), new("two"), new("three"), new("one")
        }.GetDuplicates().Single());
    }
}