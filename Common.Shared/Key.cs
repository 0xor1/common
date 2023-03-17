using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Common.Shared;

public interface IKeyed
{
    Key Key { get; }
}

[TypeConverter(typeof(KeyConverter))]
public partial record Key
{
    public const int Min = 1;
    public const int Max = 50;
    public string Value { get; }

    public Key(string value)
    {
        Validate(value);
        Value = value;
    }

    [GeneratedRegex(@"__")]
    public static partial Regex NoDoubleUnderscores();

    [GeneratedRegex(@"^[a-z]")]
    public static partial Regex StartLetter();

    [GeneratedRegex(@"_$")]
    public static partial Regex EndUnderscore();

    [GeneratedRegex(@"^[a-z0-9_]+$")]
    public static partial Regex ValidChars();

    [GeneratedRegex(@"[^a-z0-9]")]
    public static partial Regex NotLowerAlphaNumeric();

    [GeneratedRegex(@"_+")]
    public static partial Regex ConsecutiveUnderscores();

    private static void Validate(string str)
    {
        if (str.Length is < Min or > Max)
        {
            throw new InvalidDataException($"{str} must be {Min} to {Max} characters long");
        }
        if (NoDoubleUnderscores().IsMatch(str))
        {
            throw new InvalidDataException($"{str} must not contain double underscores");
        }
        if (!StartLetter().IsMatch(str))
        {
            throw new InvalidDataException($"{str} must start with a letter");
        }
        if (EndUnderscore().IsMatch(str))
        {
            throw new InvalidDataException($"{str} must not end with _");
        }
        if (!ValidChars().IsMatch(str))
        {
            throw new InvalidDataException($"{str} must only container lower case letters, digits and underscore");
        }
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
        {
            return new(k);
        }
        k = k.ToLower();
        k = NotLowerAlphaNumeric().Replace(k, "_");
        k = ConsecutiveUnderscores().Replace(k, "_");
        k = k.Trim('_');
        if (k == "")
        {
            k = "a";
        }
        if (k.Length > Max)
        {
            k = k.Substring(0, 50);
            k = k.Trim('_');
        }
        return new(k);
    }

    public static explicit operator Key?(string? b) => b is null ? null : new Key(b);

    public override string ToString() => Value;
}

public class KeyConverter : TypeConverter
{
    // Overrides the CanConvertFrom method of TypeConverter.
    // The ITypeDescriptorContext interface provides the context for the
    // conversion. Typically, this interface is used at design time to
    // provide information about the design-time container.
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }
        return base.CanConvertFrom(context, sourceType);
    }

    // Overrides the ConvertFrom method of TypeConverter.
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            return new Key(s);
        }
        return base.ConvertFrom(context, culture, value);
    }

    // Overrides the ConvertTo method of TypeConverter.
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is not null)
        {
            return ((Key)value).Value;
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }
}