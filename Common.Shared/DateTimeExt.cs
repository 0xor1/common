﻿namespace Common.Shared;

public static class DateTimeExt
{
    private static readonly long Millis = TimeSpan.FromMilliseconds(1).Ticks;

    public static DateTime UtcNowMilli()
    {
        var dt = DateTime.UtcNow;
        return dt.AddTicks(-(dt.Ticks % Millis));
    }

    public static DateTime Zero()
    {
        return new(1, 1, 1, 0, 0, 0);
    }

    public static bool IsZero(this DateTime dt)
    {
        return dt == Zero();
    }

    public static bool IsntZero(this DateTime dt)
    {
        return !dt.IsZero();
    }

    public static double SecondsSince(this DateTime dt)
    {
        return DateTime.UtcNow.Subtract(dt).TotalSeconds;
    }

    public static double MinutesSince(this DateTime dt)
    {
        return DateTime.UtcNow.Subtract(dt).TotalMinutes;
    }

    public static double DaysSince(this DateTime dt)
    {
        return DateTime.UtcNow.Subtract(dt).TotalDays;
    }
}
