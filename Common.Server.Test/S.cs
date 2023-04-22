using Common.Shared;

namespace Common.Server.Test;

public static class S
{
    public const string EN = "en";
    public const string DefaultLang = EN;
    public const string DefaultDateFmt = "yyyy-MM-dd";
    public const string DefaultTimeFmt = "HH:mm";

    public static readonly IReadOnlyList<Lang> SupportedLangs = new List<Lang>()
    {
        new(EN, "English")
    };

    public static readonly IReadOnlyList<DateTimeFmt> SupportedDateFmts = new List<DateTimeFmt>()
    {
        new(DefaultDateFmt)
    };

    public static readonly IReadOnlyList<DateTimeFmt> SupportedTimeFmts = new List<DateTimeFmt>()
    {
        new(DefaultTimeFmt)
    };

    public static readonly Common.Shared.S Inst;

    static S()
    {
        Inst = new Strings(
            DefaultLang,
            DefaultDateFmt,
            DefaultTimeFmt,
            SupportedLangs,
            SupportedDateFmts,
            SupportedTimeFmts,
            new Dictionary<string, IReadOnlyDictionary<string, TemplatableString>>()
        );
    }
}