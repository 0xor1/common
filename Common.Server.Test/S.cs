using Common.Shared;

namespace Common.Server.Test;

public static class S
{
    public const string EN = "en";
    public const string ES = "es";
    public const string DefaultLang = EN;
    public const string DefaultDateFmt = "yyyy-MM-dd";
    public const string DefaultTimeFmt = "HH:mm";
    public const string DefaultThousandsSeparator = ",";
    public const string DefaultDecimalSeparator = ".";

    public static readonly IReadOnlyList<Lang> SupportedLangs = new List<Lang>()
    {
        new(EN, "English"),
        new(ES, "Español")
    };

    public static readonly IReadOnlyList<DateTimeFmt> SupportedDateFmts = new List<DateTimeFmt>()
    {
        new(DefaultDateFmt),
        new("dd-MM-yyyy"),
        new("MM-dd-yyyy")
    };

    public static readonly IReadOnlyList<DateTimeFmt> SupportedTimeFmts = new List<DateTimeFmt>()
    {
        new(DefaultTimeFmt),
        new("h:mmtt")
    };

    public static readonly IReadOnlyList<string> SupportedThousandsSeparators = new List<string>()
    {
        DefaultThousandsSeparator,
        DefaultDecimalSeparator
    };

    public static readonly IReadOnlyList<string> SupportedDecimalSeparators = new List<string>()
    {
        DefaultDecimalSeparator,
        DefaultThousandsSeparator
    };

    public static readonly Common.Shared.S Inst;

    static S()
    {
        Inst = new Strings(
            DefaultLang,
            DefaultDateFmt,
            DefaultTimeFmt,
            DefaultThousandsSeparator,
            DefaultDecimalSeparator,
            SupportedLangs,
            SupportedDateFmts,
            SupportedTimeFmts,
            SupportedThousandsSeparators,
            SupportedDecimalSeparators,
            new Dictionary<string, IReadOnlyDictionary<string, TemplatableString>>()
            {
                { EN, new Dictionary<string, TemplatableString>() },
                { ES, new Dictionary<string, TemplatableString>() }
            }
        );
    }
}
