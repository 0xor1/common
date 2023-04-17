namespace Common.Shared.Test;

public class TaskExtTests
{
    [Fact]
    public async void WhenAll_ExceptionContainsAllSubExceptions()
    {
        var taskCompletionSourceA = new TaskCompletionSource<int>();
        taskCompletionSourceA.TrySetException(new List<Exception>()
        {
            new ("a")
        });
        var taskCompletionSourceB = new TaskCompletionSource<int>();
        taskCompletionSourceB.TrySetException(new List<Exception>()
        {
            new ("b")
        });
        try
        {
            await TaskExt.WhenAll(taskCompletionSourceA.Task, taskCompletionSourceB.Task);
        }
        catch(AggregateException ex)
        {
            Assert.Equal(2, ex.InnerExceptions.Count);
        }
    }
}