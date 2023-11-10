namespace Common.Shared;

public static class Do
{
    public static void If(bool condition, Action fn)
    {
        if (condition)
            fn();
    }

    public static async Task IfAsync(bool condition, Func<Task> fn)
    {
        if (condition)
            await fn();
    }
}
