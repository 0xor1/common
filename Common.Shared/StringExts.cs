using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Common.Shared;

public static class StringExts
{
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
        => string.IsNullOrEmpty(str);
    
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str)
        => string.IsNullOrWhiteSpace(str);
}