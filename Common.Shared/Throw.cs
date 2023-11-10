namespace Common.Shared;

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
