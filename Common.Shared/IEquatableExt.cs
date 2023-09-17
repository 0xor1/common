namespace Common.Shared;

public static class IEnumerableExt
{
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
