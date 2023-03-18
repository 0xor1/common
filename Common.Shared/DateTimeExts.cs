namespace Common.Shared;

public static class DateTimeExts
{
    public static DateTime Zero() => new DateTime(1, 1, 1, 0, 0, 0);

    public static bool IsZero(this DateTime dt) => dt == Zero();

    public static bool IsntZero(this DateTime dt) => !dt.IsZero();

    public static double SecondsSince(this DateTime dt) =>
        DateTime.UtcNow.Subtract(dt).TotalSeconds;

    public static double MinutesSince(this DateTime dt) =>
        DateTime.UtcNow.Subtract(dt).TotalMinutes;

    public static double DaysSince(this DateTime dt) => DateTime.UtcNow.Subtract(dt).TotalDays;
}
