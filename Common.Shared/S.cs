namespace Common.Shared;

public interface S
{
    public string DefaultLang { get; }
    public DateFmt DefaultDateFmt { get; }
    public string DefaultTimeFmt { get; }
    public string DefaultDateSeparator { get; }
    public string DefaultThousandsSeparator { get; }
    public string DefaultDecimalSeparator { get; }
    public IReadOnlyList<Lang> SupportedLangs { get; }
    public IReadOnlyList<DateTimeFmt> SupportedTimeFmts { get; }
    public IReadOnlyList<string> SupportedDateSeparators { get; }
    public IReadOnlyList<string> SupportedThousandsSeparators { get; }
    public IReadOnlyList<string> SupportedDecimalSeparators { get; }

    public IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, TemplatableString>
    > Library { get; }

    string Get(string lang, string key, object? model = null);
    bool TryGet(string lang, string key, out string res, object? model = null);
    string GetOr(string lang, string key, string def, object? model = null);
    string GetOrAddress(string lang, string key, object? model = null);
    string BestLang(string acceptLangsHeader);
    string BestLang(IReadOnlyList<string> langPrefs);
    string BestLang(
        IReadOnlyList<string> langPrefs,
        IReadOnlyList<string> supportedLangs,
        string defaultLang
    );
}
