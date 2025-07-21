namespace Common.Shared;

public record DateTimeFmt
{
    private static readonly DateTime dt = new(DateTime.UtcNow.Year, 1, 21, 16, 1, 1);
    public string Value { get; init; }
    public DateFmt? DateFmtValue { get; init; }

    public DateTimeFmt(string value)
    {
        Value = value;
    }

    public DateTimeFmt(DateFmt fmt, string sep)
    {
        DateFmtValue = fmt;
        Value = DateFmtExt.ToString(fmt, sep);
    }

    public override string ToString()
    {
        return dt.ToString(Value);
    }
}

public enum DateFmt
{
    YMD,
    DMY,
    MDY,
}

public static class DateFmtExt
{
    public static List<DateTimeFmt> GetFmts(string sep)
    {
        return new List<DateTimeFmt>()
        {
            new(DateFmt.YMD, sep),
            new(DateFmt.DMY, sep),
            new(DateFmt.MDY, sep),
        };
    }

    public static string ToString(this DateFmt fmt, string sep) =>
        fmt switch
        {
            DateFmt.YMD => $"yyyy{sep}MM{sep}dd",
            DateFmt.DMY => $"dd{sep}MM{sep}yyyy",
            DateFmt.MDY => $"MM{sep}dd{sep}yyyy",
            _ => throw new ArgumentOutOfRangeException(nameof(fmt), fmt, null),
        };
}
