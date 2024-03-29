﻿using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web;

namespace Common.Shared;

public static class StringAndBytesExt
{
    public static string? Ellipsis(this string? str, int max)
    {
        Throw.OpIf(max <= 3, "max must be greater than 3");
        if (str == null || str.Length <= max)
        {
            return str;
        }

        return $"{str.Substring(0, max - 3)}...";
    }

    public static Key ToKey(this string str) => Key.Force(str);

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static byte[] ToUtf8Bytes(this string? str)
    {
        return str != null ? Encoding.UTF8.GetBytes(str) : Array.Empty<byte>();
    }

    public static string FromUtf8Bytes(this byte[] bs)
    {
        return Encoding.UTF8.GetString(bs);
    }

    public static string ToB64(this byte[] arg)
    {
        var s = Convert.ToBase64String(arg); // Regular base64 encoder
        s = s.TrimEnd('='); // Remove any trailing '='s
        s = s.Replace('+', '-'); // 62nd char of encoding
        s = s.Replace('/', '_'); // 63rd char of encoding
        return s;
    }

    public static byte[] FromB64(this string arg)
    {
        var s = arg;
        s = s.Replace('-', '+'); // 62nd char of encoding
        s = s.Replace('_', '/'); // 63rd char of encoding
        switch (s.Length % 4) // Pad with trailing '='s
        {
            case 0:
                break; // No pad chars in this case
            case 2:
                s += "==";
                break; // Two pad chars
            case 3:
                s += "=";
                break; // One pad char
            default:
                throw new Exception("Illegal base64url string!");
        }

        return Convert.FromBase64String(s); // Standard base64 decoder
    }

    public static string UrlEncode(this string s) => HttpUtility.UrlEncode(s);

    public static string ContentType(this string t) =>
        t.IsNullOrWhiteSpace() ? "application/octet-stream" : t;
}
