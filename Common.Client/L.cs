using Common.Shared;

namespace Common.Client;

public interface L
{
    private static L? _inst;

    void Config(string lang, string date, string time);

    // S for String
    public string S(string key, object? model = null);

    // D for Date
    public string D(DateTime dt);

    // T for Time
    public string T(DateTime dt);

    public static L Init(S s)
    {
        return _inst ??= new LImpl(s);
    }
}

// L for Localizer
internal class LImpl : L
{
    private readonly S _s;
    private string _dateFmt;
    private string _lang;
    private string _timeFmt;


    internal LImpl(S s)
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
    public string S(string key, object? model = null)
    {
        return _s.GetOrAddress(_lang, key, model);
    }

    // D for Date
    public string D(DateTime dt)
    {
        return dt.ToLocalTime().ToString(_dateFmt);
    }

    // T for Time
    public string T(DateTime dt)
    {
        return dt.ToLocalTime().ToString(_timeFmt);
    }
}