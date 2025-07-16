using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MessagePack;

namespace Common.Shared;

[TypeConverter(typeof(KeyConverter))]
[MessagePackObject]
public partial record Key: ISpanParsable<Key>
{
    public const int Min = 1;
    public const int Max = 50;

    [SerializationConstructor]
    public Key(string value)
    {
        Validate(value);
        Value = value;
    }

    [Key(0)]
    public string Value { get; }

    [GeneratedRegex(@"__")]
    public static partial Regex NoDoubleUnderscores();

    [GeneratedRegex(@"^[a-z]")]
    private static partial Regex StartLetter();

    [GeneratedRegex(@"_$")]
    private static partial Regex EndUnderscore();

    [GeneratedRegex(@"^[a-z0-9_]+$")]
    private static partial Regex ValidChars();

    [GeneratedRegex(@"[^a-z0-9]")]
    private static partial Regex NotLowerAlphaNumeric();

    [GeneratedRegex(@"_+")]
    private static partial Regex ConsecutiveUnderscores();

    [GeneratedRegex(@"(^|_)[a-z]")]
    private static partial Regex ToPascalRx();

    [GeneratedRegex(@"(_)[a-z]")]
    private static partial Regex ToCamelRx();

    private static void Validate(string str)
    {
        if (str.Length is < Min or > Max)
            throw new InvalidDataException($"{str} must be {Min} to {Max} characters long");
        if (NoDoubleUnderscores().IsMatch(str))
            throw new InvalidDataException($"{str} must not contain double underscores");
        if (!StartLetter().IsMatch(str))
            throw new InvalidDataException($"{str} must start with a letter");
        if (EndUnderscore().IsMatch(str))
            throw new InvalidDataException($"{str} must not end with _");
        if (!ValidChars().IsMatch(str))
            throw new InvalidDataException(
                $"{str} must only container lower case letters, digits and underscore"
            );
    }

    public static bool IsValid(string maybeKey)
    {
        try
        {
            Validate(maybeKey);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static Key Force(string k)
    {
        if (IsValid(k))
            return new Key(k);
        k = k.ToLower();
        k = NotLowerAlphaNumeric().Replace(k, "_");
        k = ConsecutiveUnderscores().Replace(k, "_");
        k = k.Trim('_');
        if (k == "")
            k = "a";
        if (k.Length > Max)
        {
            k = k.Substring(0, 50);
            k = k.Trim('_');
        }

        return new Key(k);
    }

    public static explicit operator Key?(string? b)
    {
        return b is null ? null : new Key(b);
    }

    public string ToPascal() =>
        ToPascalRx().Replace(Value, m => m.ToString().ToUpper()).Replace("_", "");

    public string ToCamel() =>
        ToCamelRx().Replace(Value, m => m.ToString().ToUpper()).Replace("_", "");

    public override string ToString()
    {
        return Value;
    }
    public static Key Parse(string s, IFormatProvider? provider)
    {
        return new Key(s);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Key result)
    {
        try
        {
            s.NotNull();
            result = new Key(s);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public static Key Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return Parse(s.ToString(), provider);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Key result)
    {
        return TryParse(s.ToString(), provider, out result);
    }
}
