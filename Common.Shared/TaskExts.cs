namespace Common.Shared;

public static class TaskExts
{
    public static Task<T> Task<T>(this T obj)
    {
        return System.Threading.Tasks.Task.FromResult(obj);
    }
}
