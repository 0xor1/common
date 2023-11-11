using Fluid;

namespace Common.Shared;

public class Strings : S
{
    private static readonly SemaphoreSlim _ss = new(1, 1);
    public static readonly FluidParser Parser = new();

    public Strings(
        string defaultLang,
        string defaultDateFmt,
        string defaultTimeFmt,
        string defaultThousandsSeparator,
        string defaultDecimalSeparator,
        IReadOnlyList<Lang> supportedLangs,
        IReadOnlyList<DateTimeFmt> supportedDateFmts,
        IReadOnlyList<DateTimeFmt> supportedTimeFmts,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, TemplatableString>> library
    )
    {
        DefaultLang = defaultLang;
        DefaultDateFmt = defaultDateFmt;
        DefaultTimeFmt = defaultTimeFmt;
        DefaultThousandsSeparator = defaultThousandsSeparator;
        DefaultDecimalSeparator = defaultDecimalSeparator;
        SupportedLangs = supportedLangs;
        SupportedLangCodes = supportedLangs.Select(x => x.Code).ToList();
        SupportedDateFmts = supportedDateFmts;
        SupportedTimeFmts = supportedTimeFmts;
        Library = library;
    }

    public string DefaultLang { get; }
    public string DefaultDateFmt { get; }
    public string DefaultTimeFmt { get; }
    public string DefaultThousandsSeparator { get; }
    public string DefaultDecimalSeparator { get; }
    public IReadOnlyList<Lang> SupportedLangs { get; }
    public IReadOnlyList<string> SupportedLangCodes { get; }
    public IReadOnlyList<DateTimeFmt> SupportedDateFmts { get; }
    public IReadOnlyList<DateTimeFmt> SupportedTimeFmts { get; }

    public IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, TemplatableString>
    > Library { get; }

    public string Get(string lang, string key, object? model = null)
    {
        Throw.DataIf(!Library.ContainsKey(lang), $"I18n.S doesnt contain lang {lang}");
        Throw.DataIf(
            !Library[lang].ContainsKey(key),
            $"I18n.S doesnt contain key: {key} for lang: {lang}"
        );
        return RenderWithModel(Library[lang][key], model);
    }

    public bool TryGet(string lang, string key, out string res, object? model = null)
    {
        res = "";
        if (!Library.ContainsKey(lang))
            return false;

        if (!Library[lang].ContainsKey(key))
            return false;
        res = RenderWithModel(Library[lang][key], model);
        return true;
    }

    public string GetOr(string lang, string key, string def, object? model = null)
    {
        if (!Library.ContainsKey(lang))
            return def;

        if (!Library[lang].ContainsKey(key))
            return def;

        return RenderWithModel(Library[lang][key], model);
    }

    public string GetOrAddress(string lang, string key, object? model = null)
    {
        if (!Library.ContainsKey(lang) || !Library[lang].ContainsKey(key))
            return $"{lang}:{key}";

        return RenderWithModel(Library[lang][key], model);
    }

    public string BestLang(string acceptLangsHeader) =>
        BestLang(acceptLangsHeader.Replace(" ", "").Split(",").ToList());

    public string BestLang(IReadOnlyList<string> langPrefs) =>
        BestLang(langPrefs, SupportedLangCodes, DefaultLang);

    public string BestLang(
        IReadOnlyList<string> langPrefs,
        IReadOnlyList<string> supportedLangs,
        string defaultLang
    )
    {
        // direct matches
        foreach (var lang in langPrefs)
            if (supportedLangs.Contains(lang))
                return lang;
        // root match
        foreach (var lang in langPrefs)
        {
            var root = lang.Split("-").First();
            if (supportedLangs.Contains(root))
                return root;
        }

        return defaultLang;
    }

    private static string RenderWithModel(TemplatableString tplStr, object? model)
    {
        if (model == null)
            return tplStr.Raw;

        if (tplStr.Template == null)
            try
            {
                _ss.Wait();
                tplStr.Template = Parser.Parse(tplStr.Raw);
            }
            finally
            {
                _ss.Release();
            }

        return tplStr.Template.Render(new TemplateContext(model));
    }
}