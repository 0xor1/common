using Fluid;

namespace Common.Shared;

public class Strings : S
{
    private static readonly SemaphoreSlim _ss = new(1, 1);
    public static readonly FluidParser Parser = new();

    public static Strings FromCommon(
        Dictionary<string, Dictionary<string, TemplatableString>> library
    )
    {
        var cmn = I18n.S.Inst;
        var cmnKeys = cmn.Library.Keys.ToHashSet();
        var lKeys = library.Keys.ToHashSet();
        Throw.DataIf(
            !lKeys.IsSubsetOf(cmnKeys),
            "library contains languages not supported by common"
        );
        library.ForEach(
            kvp => cmn.Library[kvp.Key].ForEach(cmnKvp => kvp.Value.Add(cmnKvp.Key, cmnKvp.Value))
        );
        return new Strings(
            cmn.DefaultLang,
            cmn.DefaultDateFmt,
            cmn.DefaultTimeFmt,
            cmn.DefaultDateSeparator,
            cmn.DefaultThousandsSeparator,
            cmn.DefaultDecimalSeparator,
            cmn.SupportedLangs.Where(x => library.Keys.Contains(x.Code)).ToList(),
            cmn.SupportedTimeFmts,
            cmn.SupportedDateSeparators,
            cmn.SupportedThousandsSeparators,
            cmn.SupportedDecimalSeparators,
            library
                .Select(x => x)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value as IReadOnlyDictionary<string, TemplatableString>
                )
        );
    }

    public Strings(
        string defaultLang,
        DateFmt defaultDateFmt,
        string defaultTimeFmt,
        string defaultDateSeparator,
        string defaultThousandsSeparator,
        string defaultDecimalSeparator,
        IReadOnlyList<Lang> supportedLangs,
        IReadOnlyList<DateTimeFmt> supportedTimeFmts,
        IReadOnlyList<string> supportedDateSeparators,
        IReadOnlyList<string> supportedThousandsSeparators,
        IReadOnlyList<string> supportedDecimalSeparators,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, TemplatableString>> library
    )
    {
        Throw.DataIf(
            defaultThousandsSeparator == defaultDecimalSeparator,
            "default thousands separator is the same as default decimal separator"
        );
        var firstStrs = library.First();
        var firstStrLang = firstStrs.Key;
        var firstStrKeys = firstStrs.Value.Keys.ToHashSet();
        library.ForEach(
            (kvp) =>
                Throw.DataIf(
                    !firstStrKeys.SetEquals(kvp.Value.Keys.ToHashSet()),
                    $"{firstStrLang} and {kvp.Key} langs don't contain matching string keys"
                )
        );
        DefaultLang = defaultLang;
        DefaultDateFmt = defaultDateFmt;
        DefaultTimeFmt = defaultTimeFmt;
        DefaultDateSeparator = defaultDateSeparator;
        DefaultThousandsSeparator = defaultThousandsSeparator;
        DefaultDecimalSeparator = defaultDecimalSeparator;
        SupportedLangs = supportedLangs;
        SupportedLangCodes = supportedLangs.Select(x => x.Code).ToList();
        SupportedTimeFmts = supportedTimeFmts;
        SupportedDateSeparators = supportedDateSeparators;
        SupportedThousandsSeparators = supportedThousandsSeparators;
        SupportedDecimalSeparators = supportedDecimalSeparators;
        Library = library;
    }

    public string DefaultLang { get; }
    public DateFmt DefaultDateFmt { get; }
    public string DefaultTimeFmt { get; }
    public string DefaultDateSeparator { get; }
    public string DefaultThousandsSeparator { get; }
    public string DefaultDecimalSeparator { get; }
    public IReadOnlyList<Lang> SupportedLangs { get; }
    public IReadOnlyList<string> SupportedLangCodes { get; }
    public IReadOnlyList<DateTimeFmt> SupportedTimeFmts { get; }
    public IReadOnlyList<string> SupportedDateSeparators { get; }
    public IReadOnlyList<string> SupportedThousandsSeparators { get; }
    public IReadOnlyList<string> SupportedDecimalSeparators { get; }

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
