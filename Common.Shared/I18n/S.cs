﻿namespace Common.Shared.I18n;

public static partial class S
{
    public const string EN = "en";
    public const string ES = "es";
    public const string FR = "fr";
    public const string DE = "de";
    public const string IT = "it";
    public const string DefaultLang = EN;
    public const DateFmt DefaultDateFmt = Common.Shared.DateFmt.YMD;
    public const string DefaultTimeFmt = "HH:mm";
    public const string DefaultDateSeparator = "-";
    public const string DefaultThousandsSeparator = ",";
    public const string DefaultDecimalSeparator = ".";

    public static readonly IReadOnlyList<Lang> SupportedLangs = new List<Lang>()
    {
        new(EN, "English"),
        new(ES, "Español"),
        new(FR, "Français"),
        new(DE, "Deutsch"),
        new(IT, "Italiano"),
    };

    public static readonly IReadOnlyList<DateTimeFmt> SupportedTimeFmts = new List<DateTimeFmt>()
    {
        new(DefaultTimeFmt),
        new("h:mmtt"),
    };

    public static readonly IReadOnlyList<string> SupportedDateSeparators = new List<string>()
    {
        DefaultDateSeparator,
        "/",
        ".",
    };

    public static readonly IReadOnlyList<string> SupportedThousandsSeparators = new List<string>()
    {
        DefaultDecimalSeparator,
        DefaultThousandsSeparator,
    };

    public static readonly IReadOnlyList<string> SupportedDecimalSeparators = new List<string>()
    {
        DefaultThousandsSeparator,
        DefaultDecimalSeparator,
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
            Library
        );
    }
}
