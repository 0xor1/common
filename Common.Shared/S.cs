using Fluid;

namespace Common.Shared;

public record TemplatableString(string Raw)
{
    internal IFluidTemplate? Template { get; set; }
}

public record Lang(string Code, string NativeName)
{
    public override string ToString()
    {
        return NativeName;
    }
}

public record DateTimeFmt(string Value)
{
    private static readonly DateTime dt = new(DateTime.UtcNow.Year, 1, 21, 16, 1, 1);

    public override string ToString()
    {
        return dt.ToString(Value);
    }
}

public interface S
{
    // common string keys used in shared code.
    public const string Invalid = "invalid";
    public const string RpcUnknownEndpoint = "rpc_unknown_endpoint";
    public const string UnexpectedError = "unexpected_error";
    public const string EntityNotFound = "entity_not_found";
    public const string InsufficientPermission = "insufficient_permission";
    public const string ApiError = "api_error";
    public const string MinMaxNullArgs = "min_max_null_args";
    public const string MinMaxReversedArgs = "min_max_reversed_args";
    public const string BadRequest = "bad_request";
    public const string RequestBodyTooLarge = "request_body_too_large";

    // auth specific
    public const string AuthInvalidEmail = "auth_invalid_email";
    public const string AuthInvalidPwd = "auth_invalid_pwd";
    public const string AuthLessThan8Chars = "auth_less_than_8_chars";
    public const string AuthNoLowerCaseChar = "auth_no_lower_case_char";
    public const string AuthNoUpperCaseChar = "auth_no_upper_case_char";
    public const string AuthNoDigit = "auth_no_digit";
    public const string AuthNoSpecialChar = "auth_no_special_char";
    public const string AuthInvalidEmailCode = "auth_invalid_email_code";
    public const string AuthInvalidResetPwdCode = "auth_invalid_reset_pwd_code";
    public const string AuthAccountNotVerified = "auth_account_not_verified";
    public const string AuthAlreadyAuthenticated = "auth_already_authenticated";
    public const string AuthNotAuthenticated = "auth_not_authenticated";
    public const string AuthAttemptRateLimit = "auth_attempt_rate_limit";
    public const string AuthConfirmEmailSubject = "auth_confirm_email_subject";
    public const string AuthConfirmEmailHtml = "auth_confirm_email_html";
    public const string AuthConfirmEmailText = "auth_confirm_email_text";
    public const string AuthResetPwdSubject = "auth_reset_pwd_subject";
    public const string AuthResetPwdHtml = "auth_reset_pwd_html";
    public const string AuthResetPwdText = "auth_reset_pwd_text";
    public const string AuthFcmTopicInvalid = "auth_fcm_topic_invalid";
    public const string AuthFcmTokenInvalid = "auth_fcm_token_invalid";
    public const string AuthFcmNotEnabled = "auth_fcm_not_enabled";
    public const string AuthFcmMessageInvalid = "auth_fcm_message_invalid";

    public string DefaultLang { get; }
    public string DefaultDateFmt { get; }
    public string DefaultTimeFmt { get; }
    public IReadOnlyList<Lang> SupportedLangs { get; }
    public IReadOnlyList<DateTimeFmt> SupportedDateFmts { get; }
    public IReadOnlyList<DateTimeFmt> SupportedTimeFmts { get; }

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

public class Strings : S
{
    private static readonly SemaphoreSlim _ss = new(1, 1);
    public static readonly FluidParser Parser = new();

    public Strings(
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
        SupportedLangCodes = supportedLangs.Select(x => x.Code).ToList();
        SupportedDateFmts = supportedDateFmts;
        SupportedTimeFmts = supportedTimeFmts;
        Library = library;
    }

    public string DefaultLang { get; }
    public string DefaultDateFmt { get; }
    public string DefaultTimeFmt { get; }
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
