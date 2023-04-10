using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Common.Shared;

public static class NullExts
{
    public static T NotNull<T>(
        [NotNull] this T? obj,
        [CallerArgumentExpression("obj")] string? paramName = null
    )
        where T : class
    {
        ArgumentNullException.ThrowIfNull(obj, paramName);
        return obj;
    }

    public static T NotNull<T>(
        [NotNull] this T? obj,
        [CallerArgumentExpression("obj")] string? paramName = null
    )
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(obj, paramName);
        return obj.Value;
    }

    public static async Task<T> NotNull<T>(
        this Task<T?> task,
        [CallerArgumentExpression("task")] string? paramName = null
    )
        where T : class
    {
        var obj = await task;
        ArgumentNullException.ThrowIfNull(obj, paramName);
        return obj;
    }

    public static async ValueTask<T> NotNull<T>(
        this ValueTask<T?> task,
        [CallerArgumentExpression("task")] string? paramName = null
    )
        where T : class
    {
        var obj = await task;
        ArgumentNullException.ThrowIfNull(obj, paramName);
        return obj;
    }

    public static bool IsNull<T>([NotNullWhen(false)] this T? obj)
        where T : class
    {
        return obj is null;
    }

    public static bool IsNull<T>([NotNullWhen(false)] this T? obj)
        where T : struct
    {
        return !obj.HasValue;
    }

    public static bool IsntNull<T>([NotNullWhen(true)] this T? obj)
        where T : class
    {
        return !obj.IsNull();
    }

    public static bool IsntNull<T>([NotNullWhen(true)] this T? obj)
        where T : struct
    {
        return !obj.IsNull();
    }
}
