using Fluid;

namespace Common.Shared.I18n;

public record TemplatableString(string Raw)
{
    internal IFluidTemplate? Template { get; set; }
}

public record Lang(string Code, string NativeName)
{
    public override string ToString() => NativeName;
}

public record DateTimeFmt(string Value)
{
    private static readonly DateTime dt = new(DateTime.UtcNow.Year, 1, 21, 16, 1, 1);

    public override string ToString() => dt.ToString(Value);
}

public class S
{
    private static S? _inst;
    public static readonly FluidParser Parser = new();
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    
    // common string keys used in shared code.
    public const string Invalid = "invalid";
    public const string InvalidEmail = "invalid_email";
    public const string InvalidPwd = "invalid_pwd";
    public const string LessThan8Chars = "less_than_8_chars";
    public const string NoLowerCaseChar = "no_lower_case_char";
    public const string NoUpperCaseChar = "no_upper_case_char";
    public const string NoDigit = "no_digit";
    public const string NoSpecialChar = "no_special_char";
    public const string UnexpectedError = "unexpected_error";

    public static S Get() => _inst ?? throw new InvalidSetupException("S has not be initialized yet");
    public static S Init(
        string defaultLang,
        string defaultDateFmt,
        string defaultTimeFmt,
        IReadOnlyList<Lang> supportedLangs,
        IReadOnlyList<DateTimeFmt> supportedDateFmts,
        IReadOnlyList<DateTimeFmt> supportedTimeFmts,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, TemplatableString>> library
    )
    {
        _semaphoreSlim.Wait();
        try
        {
            Throw.OpIf(
                _inst != null,
                "Singleton I18n.S has already been initialised, you should initialise I18n.S only once in your startup code."
            );
            _inst = new S(
                defaultLang,
                defaultDateFmt,
                defaultTimeFmt,
                supportedLangs,
                supportedDateFmts,
                supportedTimeFmts,
                library
            );
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        return _inst;
    }

    public readonly string DefaultLang;
    public readonly string DefaultDateFmt;
    public readonly string DefaultTimeFmt;
    public readonly IReadOnlyList<Lang> SupportedLangs;
    public readonly IReadOnlyList<DateTimeFmt> SupportedDateFmts;
    public readonly IReadOnlyList<DateTimeFmt> SupportedTimeFmts;
    private readonly IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, TemplatableString>
    > Library;

    private S(
        string defaultLang,
        string defaultDateFmt,
        string defaultTimeFmt,
        IReadOnlyList<Lang> supportedLangs,
        IReadOnlyList<DateTimeFmt> supportedDateFmts,
        IReadOnlyList<DateTimeFmt> supportedTimeFmts,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, TemplatableString>> library
    )
    {
        DefaultLang = defaultLang;
        DefaultDateFmt = defaultDateFmt;
        DefaultTimeFmt = defaultTimeFmt;
        SupportedLangs = supportedLangs;
        SupportedDateFmts = supportedDateFmts;
        SupportedTimeFmts = supportedTimeFmts;
        Library = library;
    }

    public string Get(string lang, string key, object? model = null)
    {
        Throw.DataIf(!Library.ContainsKey(lang), $"I18n.S doesnt contain lang {lang}");
        Throw.DataIf(
            !Library[lang].ContainsKey(key),
            $"I18n.S doesnt contain key: {key} for lang: {lang}"
        );
        return RenderWithModel(Library[lang][key], model);
    }

    private static string RenderWithModel(TemplatableString tplStr, object? model)
    {
        if (model == null)
        {
            return tplStr.Raw;
        }

        if (tplStr.Template == null)
        {
            try
            {
                _semaphoreSlim.Wait();
                tplStr.Template = Parser.Parse(tplStr.Raw);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        return tplStr.Template.Render(new TemplateContext(model));
    }

    public bool TryGet(string lang, string key, out string res, object? model = null)
    {
        res = "";
        if (!Library.ContainsKey(lang))
        {
            return false;
        }

        if (!Library[lang].ContainsKey(key))
        {
            return false;
        }
        res = RenderWithModel(Library[lang][key], model);
        return true;
    }

    public string GetOr(string lang, string key, string def, object? model = null)
    {
        if (!Library.ContainsKey(lang))
        {
            return def;
        }

        if (!Library[lang].ContainsKey(key))
        {
            return def;
        }

        return RenderWithModel(Library[lang][key], model);
    }

    public string GetOrAddress(string lang, string key, object? model = null)
    {
        if (!Library.ContainsKey(lang) || !Library[lang].ContainsKey(key))
        {
            return $"{lang}:{key}";
        }

        return RenderWithModel(Library[lang][key], model);
    }

    public string BestLang(string acceptLangsHeader)
    {
        var langs = acceptLangsHeader.Replace(" ", "").Split(",");
        // direct matches
        foreach (var lang in langs)
        {
            if (Library.ContainsKey(lang))
            {
                return lang;
            }
        }
        // root match
        foreach (var lang in langs)
        {
            var root = lang.Split("-").First();
            if (Library.ContainsKey(root))
            {
                return root;
            }
        }
        return DefaultLang;
    }
}
