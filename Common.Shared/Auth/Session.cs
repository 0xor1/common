using MessagePack;
using Str = Common.Shared.I18n.S;

namespace Common.Shared.Auth;

[MessagePackObject]
public record Session(
    [property: Key(0)] string Id,
    [property: Key(1)] bool IsAuthed,
    [property: Key(2)] DateTime StartedOn,
    [property: Key(3)] bool RememberMe,
    [property: Key(4)] string Lang,
    [property: Key(5)] DateFmt DateFmt,
    [property: Key(6)] string TimeFmt,
    [property: Key(7)] string DateSeparator,
    [property: Key(8)] string ThousandsSeparator,
    [property: Key(9)] string DecimalSeparator,
    [property: Key(10)] bool FcmEnabled
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

    public Session ToApi() => this;
}
