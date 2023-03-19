using Common.Shared;
using Common.Shared.I18n;

namespace Common.Client;

// L for Localizer
public class L
{
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private static L _inst;
    private string _lang;
    private string _dateFmt;
    private string _timeFmt;
    private readonly S _s;
    
    public static L Get() => _inst ?? throw new InvalidSetupException("L has not be initialized yet");
    
    public static L Init(S s)
    {
        _semaphoreSlim.Wait();
        try
        {
            Throw.OpIf(
                _inst != null,
                "Singleton L has already been initialised, you should initialise L only once in your startup code."
            );
            _inst = new L(s);
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        return _inst;
    }
    
    private L(S s)
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
