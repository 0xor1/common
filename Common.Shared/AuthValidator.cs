using System.Text.RegularExpressions;

namespace Common.Shared;

public static partial class AuthValidator
{
    public static ValidationResult Email(string str)
    {
        var res = ValidationResult.New(S.AuthInvalidEmail);
        res.InvalidIf(!EmailRegex().IsMatch(str));
        return res;
    }

    public static ValidationResult Pwd(string str)
    {
        var res = ValidationResult.New(S.AuthInvalidPwd);
        res.InvalidIf(!EightOrMoreCharsRegex().IsMatch(str), S.AuthLessThan8Chars);
        res.InvalidIf(!LowerCaseRegex().IsMatch(str), S.AuthNoLowerCaseChar);
        res.InvalidIf(!UpperCaseRegex().IsMatch(str), S.AuthNoUpperCaseChar);
        res.InvalidIf(!DigitRegex().IsMatch(str), S.AuthNoDigit);
        res.InvalidIf(!SpecialCharRegex().IsMatch(str), S.AuthNoSpecialChar);
        return res;
    }

    [GeneratedRegex("^[^@]+@[^@]+\\.[^@]+$")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(".{8,}")]
    private static partial Regex EightOrMoreCharsRegex();

    [GeneratedRegex("[a-z]")]
    private static partial Regex LowerCaseRegex();

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UpperCaseRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex DigitRegex();

    [GeneratedRegex("[^a-zA-Z0-9 ]")]
    private static partial Regex SpecialCharRegex();
}
