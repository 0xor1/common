using Common.Shared;
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

internal class Localizer : L
{
    private readonly S _s;
    private string _dateFmt;
    private string _lang;
    private string _timeFmt;

    public Localizer(S s)
    {
        _s = s;
        _lang = s.DefaultLang;
        _dateFmt = s.DefaultDateFmt;
        _timeFmt = s.DefaultTimeFmt;
    }

    public void Config(string lang, string date, string time)
    {
        _lang = lang;
        _dateFmt = date;
        _timeFmt = time;
    }

    // S for String
    public string S(string key, object? model = null) => _s.GetOrAddress(_lang, key, model);

    // H for Html
    public MarkupString H(string key, object? model = null) =>
        new(_s.GetOrAddress(_lang, key, model));

    // D for Date
    public string D(DateTime dt) => dt.ToLocalTime().ToString(_dateFmt);

    // T for Time
    public string T(DateTime dt) => dt.ToLocalTime().ToString(_timeFmt);
}
