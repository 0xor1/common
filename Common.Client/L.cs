using Microsoft.AspNetCore.Components;

namespace Common.Client;

// L for Localizer
public interface L
{
    void Config(string lang, string date, string time);

    // S for String
    public string S(string key, object? model = null);

    // H for Html
    public MarkupString H(string key, object? model = null);

    // D for Date
    public string D(DateTime dt);

    // T for Time
    public string T(DateTime dt);
}
