using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Common.Shared;

public static class StringExts
{
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) =>
        string.IsNullOrEmpty(str);

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) =>
        string.IsNullOrWhiteSpace(str);

    public static IEnumerable<string> GetDuplicates(this IEnumerable<string> strs) =>
        strs.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key);
}
