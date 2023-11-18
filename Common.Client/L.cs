using Common.Shared;
using Microsoft.AspNetCore.Components;

namespace Common.Client;

// L for Localizer
public interface L
{
    void Config(
        string lang,
        DateFmt date,
        string time,
        string dateSeparator,
        string thousandsSeparator,
        string decimalSeparator
    );

    // S for String
    public string S(string key, object? model = null);

    // H for Html
    public MarkupString H(string key, object? model = null);

    // D for Date
    public string D(DateTime dt);

    // T for Time
    public string T(DateTime dt);

    // I for Int
    public string I(int i);

    // Dec for Decimal
    public string Dec(decimal d);
}
