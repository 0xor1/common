using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Common.Shared;

[TypeConverter(typeof(KeyConverter))]
public partial record Key : IConvertible
{
    public const int Min = 1;
    public const int Max = 50;

    public Key(string value)
    {
        Validate(value);
        Value = value;
    }

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

    public override string ToString()
    {
        return Value;
    }

    public TypeCode GetTypeCode()
    {
        return TypeCode.String;
    }

    public bool ToBoolean(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to bool");
    }

    public byte ToByte(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to byte");
    }

    public char ToChar(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to char");
    }

    public DateTime ToDateTime(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to datetime");
    }

    public decimal ToDecimal(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to decimal");
    }

    public double ToDouble(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to double");
    }

    public short ToInt16(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to int16");
    }

    public int ToInt32(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to int32");
    }

    public long ToInt64(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to int64");
    }

    public sbyte ToSByte(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to sbyte");
    }

    public float ToSingle(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to float");
    }

    public string ToString(IFormatProvider? provider)
    {
        return Value;
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        return Value;
    }

    public ushort ToUInt16(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to uint16");
    }

    public uint ToUInt32(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to uint32");
    }

    public ulong ToUInt64(IFormatProvider? provider)
    {
        throw new InvalidOperationException("can't convert key to uint64");
    }
}
