namespace Common.Shared.Test;

public class MinMaxTests
{
    [Fact]
    public void MinMax_Success()
    {
        var mm = new MinMax<int>(1, 2);
        Assert.Equal(1, mm.Min.NotNull());
        Assert.Equal(2, mm.Max.NotNull());
    }

    [Fact]
    public void MinMax_NullException()
    {
        try
        {
            var mm = new MinMax<int>(null, null);
        }
        catch (NullMinMaxValuesException ex)
        {
            Assert.True(true);
        }
    }

    [Fact]
    public void MinMax_ReversedException()
    {
        try
        {
            var mm = new MinMax<int>(2, 1);
        }
        catch (ReversedMinMaxValuesException ex)
        {
            Assert.True(true);
        }
    }
}
