using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Common.Shared;

public static class StringExts
{
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) =>
        string.IsNullOrEmpty(str);

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) =>
        string.IsNullOrWhiteSpace(str);

    public static byte[] Utf8Bytes(this string? str) =>
        str != null ? Encoding.UTF8.GetBytes(str) : Array.Empty<byte>();

    public static string Utf8String(this byte[] bs) => Encoding.UTF8.GetString(bs);
}
