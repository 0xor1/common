using System.Diagnostics.CodeAnalysis;

namespace Common.Shared;

public static class Do
{
    public static void If(bool condition, Action fn)
    {
        if (condition)
        {
            fn();
        }
    }

    public static async Task IfAsync(bool condition, Func<Task> fn)
    {
        if (condition)
        {
            await fn();
        }
    }
}

public static class Throw
{
    public static void If<T>(bool condition, Func<T> ex)
        where T : Exception
    {
        Do.If(condition, () => throw ex());
    }

    public static void DataIf(bool condition, string msg)
    {
        If(condition, () => new InvalidDataException(msg));
    }

    public static void OpIf(bool condition, string msg)
    {
        If(condition, () => new InvalidOperationException(msg));
    }

    public static void SetupIf(bool condition, string msg)
    {
        If(condition, () => new InvalidSetupException(msg));
    }
}

public class InvalidSetupException : Exception
{
    public InvalidSetupException(string msg)
        : base(msg) { }
}
