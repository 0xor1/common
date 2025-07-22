namespace Common.Shared.Test;

public class DateTimeExtTests
{
    [Fact]
    public void AsTask_WrapsValueInTask()
    {
        var t = DateTimeExt.UtcNowMilli();
        Assert.Equal(0, t.Microsecond);
    }
}
