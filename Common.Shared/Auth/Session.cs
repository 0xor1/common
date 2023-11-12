using Newtonsoft.Json;
using Str = Common.Shared.I18n.S;

namespace Common.Shared.Auth;

public record Session(
    string Id,
    bool IsAuthed,
    DateTime StartedOn,
    bool RememberMe,
    string Lang,
    string DateFmt,
    string TimeFmt,
    string ThousandsSeparator,
    string DecimalSeparator,
    bool FcmEnabled
) : ISession
{
    public static Session Default(
        string lang,
        string dateFmt,
        string timeFmt,
        string thousandsSeparator,
        string decimalSeparator
    ) =>
        new(
            string.Empty,
            false,
            DateTime.UtcNow,
            false,
            lang,
            dateFmt,
            timeFmt,
            thousandsSeparator,
            decimalSeparator,
            false
        );

    public static Session CommonDefault() =>
        Default(
            Str.DefaultLang,
            Str.DefaultDateFmt,
            Str.DefaultTimeFmt,
            Str.DefaultThousandsSeparator,
            Str.DefaultDecimalSeparator
        );

    [JsonIgnore]
    public bool IsAnon => !IsAuthed;
}
