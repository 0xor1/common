namespace Common.Shared;

public static class TaskExt
{
    public static Task<T> AsTask<T>(this T obj)
    {
        return Task.FromResult(obj);
    }

    // Thanks to Nick Chapsas https://www.youtube.com/watch?v=gW19LaAYczI
    public static async Task<IEnumerable<T>> WhenAll<T>(params Task<T>[] tasks)
    {
        var allTasks = Task.WhenAll(tasks);

        try
        {
            return await allTasks;
        }
        catch
        {
            throw allTasks.Exception.NotNull();
        }
    }

    // Fire and Forget
    public static async void FnF(this Task task)
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            // log errors
        }
    }
}
