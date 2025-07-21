using Common.Shared;

namespace Common.Server.Test;

public static class S
{
    public const string EN = "en";
    public const string ES = "es";
    public const string DefaultLang = EN;
    public const DateFmt DefaultDateFmt = DateFmt.YMD;
    public const string DefaultTimeFmt = "HH:mm";
    public const string DefaultDateSeparator = "-";
    public const string DefaultThousandsSeparator = ",";
    public const string DefaultDecimalSeparator = ".";

    public static readonly IReadOnlyList<Lang> SupportedLangs = new List<Lang>()
    {
        new(EN, "English"),
        new(ES, "Español"),
    };

    public static readonly IReadOnlyList<DateTimeFmt> SupportedTimeFmts = new List<DateTimeFmt>()
    {
        new(DefaultTimeFmt),
        new("h:mmtt"),
    };

    public static readonly IReadOnlyList<string> SupportedDateSeparators = new List<string>()
    {
        new(DefaultDateSeparator),
        new("/"),
        new("."),
    };

    public static readonly IReadOnlyList<string> SupportedThousandsSeparators = new List<string>()
    {
        DefaultThousandsSeparator,
        DefaultDecimalSeparator,
    };

    public static readonly IReadOnlyList<string> SupportedDecimalSeparators = new List<string>()
    {
        DefaultDecimalSeparator,
        DefaultThousandsSeparator,
    };

    public static readonly Common.Shared.S Inst;

    static S()
    {
        Inst = new Strings(
            DefaultLang,
            DefaultDateFmt,
            DefaultTimeFmt,
            DefaultDateSeparator,
            DefaultThousandsSeparator,
            DefaultDecimalSeparator,
            SupportedLangs,
            SupportedTimeFmts,
            SupportedDateSeparators,
            SupportedThousandsSeparators,
            SupportedDecimalSeparators,
            new Dictionary<string, IReadOnlyDictionary<string, TemplatableString>>()
            {
                { EN, new Dictionary<string, TemplatableString>() },
                { ES, new Dictionary<string, TemplatableString>() },
            }
        );
    }
}
