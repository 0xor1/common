namespace Common.Shared.Test;

public class MinMaxTests
{
    public record Val(int I) : IComparable<Val>
    {
        public int CompareTo(Val? other) => I.CompareTo(other?.I ?? 0);
    }

    [Fact]
    public void MinMax_Success()
    {
        var mm = new MinMax<Val>(new(1), new(2));
        Assert.Equal(1, mm.Min.NotNull().I);
        Assert.Equal(2, mm.Max.NotNull().I);
    }

    [Fact]
    public void MinMax_NullException()
    {
        try
        {
            var mm = new MinMax<Val>(null, null);
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
            var mm = new MinMax<Val>(new Val(2), new Val(1));
        }
        catch (ReversedMinMaxValuesException ex)
        {
            Assert.True(true);
        }
    }
}
