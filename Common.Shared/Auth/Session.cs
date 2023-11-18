using Newtonsoft.Json;
using Str = Common.Shared.I18n.S;

namespace Common.Shared.Auth;

public record Session(
    string Id,
    bool IsAuthed,
    DateTime StartedOn,
    bool RememberMe,
    string Lang,
    DateFmt DateFmt,
    string TimeFmt,
    string DateSeparator,
    string ThousandsSeparator,
    string DecimalSeparator,
    bool FcmEnabled
) : ISession
{
    public static Session Default(
        string lang,
        DateFmt dateFmt,
        string timeFmt,
        string dateSeparator,
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
            dateSeparator,
            thousandsSeparator,
            decimalSeparator,
            false
        );

    public static Session CommonDefault() =>
        Default(
            Str.DefaultLang,
            Str.DefaultDateFmt,
            Str.DefaultTimeFmt,
            Str.DefaultDateSeparator,
            Str.DefaultThousandsSeparator,
            Str.DefaultDecimalSeparator
        );
}
