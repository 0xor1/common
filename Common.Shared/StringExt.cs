using System.Text.RegularExpressions;
using CS = Common.Shared.I18n.S;

namespace Common.Shared;

public static class StringExt
{
    public static ValidationResult Validate(
        this string val,
        string name,
        int min,
        int max,
        List<Regex>? regexes = null
    )
    {
        var m = new
        {
            Name = name,
            Min = min,
            Max = max,
            Regexes = regexes?.Select(x => x.ToString()).ToList() ?? [],
        };
        var res = ValidationResult.New(CS.StringInvalid, m);
        res.InvalidIf(val.Length < min || val.Length > max);
        if (regexes != null)
        {
            foreach (var r in regexes)
            {
                res.InvalidIf(!r.IsMatch(val));
            }
        }

        return res;
    }
}
