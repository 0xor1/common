namespace Common.Shared;

public static class IEnumerableExt
{
    public static string Join(this IEnumerable<string>? src, string separator = ", ") =>
        string.Join(separator, src.Empty());

    public static string Join(this IEnumerable<Key>? src, string separator = ", ") =>
        string.Join(separator, src.Empty().Select(x => x.Value));

    public static IEnumerable<T> GetDuplicates<T>(this IEnumerable<T> strs)
    {
        return strs.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key);
    }

    public static void ForEach<T>(this IEnumerable<T> things, Action<T> action)
    {
        foreach (var thing in things)
        {
            action(thing);
        }
    }
}
