namespace Common.Shared;

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
    public const string AuthThousandsAndDecimalSeparatorsMatch =
        "auth_thousands_and_decimal_separators_match";
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
    public const string AuthMagicLinkSubject = "auth_magic_link_subject";
    public const string AuthMagicLinkHtml = "auth_magic_link_html";
    public const string AuthMagicLinkText = "auth_magic_link_text";
    public const string AuthFcmTopicInvalid = "auth_fcm_topic_invalid";
    public const string AuthFcmTokenInvalid = "auth_fcm_token_invalid";
    public const string AuthFcmNotEnabled = "auth_fcm_not_enabled";
    public const string AuthFcmMessageInvalid = "auth_fcm_message_invalid";

    public string DefaultLang { get; }
    public string DefaultDateFmt { get; }
    public string DefaultTimeFmt { get; }
    public string DefaultThousandsSeparator { get; }
    public string DefaultDecimalSeparator { get; }
    public IReadOnlyList<Lang> SupportedLangs { get; }
    public IReadOnlyList<DateTimeFmt> SupportedDateFmts { get; }
    public IReadOnlyList<DateTimeFmt> SupportedTimeFmts { get; }
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
