using System.Globalization;
using Common.Shared;
using Microsoft.AspNetCore.Components;

namespace Common.Client;

internal class Localizer : L
{
    private readonly S _s;
    private DateFmt _dateFmt;
    private string _lang;
    private string _timeFmt;
    private string _dateSeparator;
    private string _thousandsSeparator;
    private string _decimalSeparator;

    private string _dateFmtStr;

    public Localizer(S s)
    {
        _s = s;
        _lang = s.DefaultLang;
        _dateFmt = s.DefaultDateFmt;
        _timeFmt = s.DefaultTimeFmt;
        _dateSeparator = s.DefaultDateSeparator;
        _thousandsSeparator = s.DefaultThousandsSeparator;
        _decimalSeparator = s.DefaultDecimalSeparator;
        _dateFmtStr = new DateTimeFmt(_dateFmt, _dateSeparator.NotNull()).Value;
    }

    public void Config(
        string lang,
        DateFmt date,
        string time,
        string dateSeparator,
        string thousandsSeparator,
        string decimalSeparator
    )
    {
        _lang = lang;
        _dateFmt = date;
        _timeFmt = time;
        _dateSeparator = dateSeparator;
        _thousandsSeparator = thousandsSeparator;
        _decimalSeparator = decimalSeparator;
        _dateFmtStr = new DateTimeFmt(_dateFmt, _dateSeparator.NotNull()).Value;
    }

    // S for String
    public string S(string key, object? model = null) => _s.GetOrAddress(_lang, key, model);

    // H for Html
    public MarkupString H(string key, object? model = null) =>
        new(_s.GetOrAddress(_lang, key, model));

    // D for Date
    public string D(DateTime dt) => dt.ToLocalTime().ToString(_dateFmtStr);

    // T for Time
    public string T(DateTime dt) => dt.ToLocalTime().ToString(_timeFmt);

    // I for Int
    public string I(int i) =>
        i.ToString(
            "N",
            new NumberFormatInfo()
            {
                NumberDecimalDigits = 0,
                NumberGroupSeparator = _thousandsSeparator,
                NumberDecimalSeparator = _decimalSeparator
            }
        );

    // Dec for Decimal
    public string Dec(decimal d) =>
        d.ToString(
            "N",
            new NumberFormatInfo()
            {
                NumberGroupSeparator = _thousandsSeparator,
                NumberDecimalSeparator = _decimalSeparator
            }
        );
}
