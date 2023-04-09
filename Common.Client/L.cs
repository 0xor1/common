using Common.Shared;

namespace Common.Client;

public interface L
{
    void Config(string lang, string date, string time);
    // S for String
    public string S(string key, object? model = null);
    // D for Date
    public string D(DateTime dt);
    // T for Time
    public string T(DateTime dt);
    private static L? _inst;
    public static L Init(S s) => _inst ??= new LImpl(s);
}

// L for Localizer
internal class LImpl: L
{
    private string _lang;
    private string _dateFmt;
    private string _timeFmt;
    private readonly S _s;
    

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
    public string S(string key, object? model = null) =>
        _s.GetOrAddress(_lang, key, model);

    // D for Date
    public string D(DateTime dt) => dt.ToLocalTime().ToString(_dateFmt);

    // T for Time
    public string T(DateTime dt) => dt.ToLocalTime().ToString(_timeFmt);
}
