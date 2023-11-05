using System.Text.RegularExpressions;

namespace Common.Shared;

public record ValidationResult
{
    public static ValidationResult New(string msgKey = S.Invalid, object? msgModel = null)
    {
        return new(true, msgKey, msgModel);
    }

    private ValidationResult(bool valid, string msgKey = S.Invalid, object? msgModel = null)
    {
        _valid = valid;
        Message = new Message(msgKey, msgModel);
    }

    private readonly bool _valid;
    public bool Valid => _valid && _subResults.All(x => x.Valid);
    public Message Message { get; private set; }
    private List<ValidationResult> _subResults = new();
    public IReadOnlyList<ValidationResult> SubResults => _subResults;

    public bool InvalidIf(bool condition, string? msgKey = null, object? msgModel = null)
    {
        if (condition)
        {
            if (!msgKey.IsNullOrEmpty())
            {
                _subResults.Add(new(false, msgKey, msgModel));
            }
        }
        return condition;
    }

    public ValidationResult NewSubResult(string msgKey = S.Invalid, object? msgModel = null)
    {
        var res = new ValidationResult(true, msgKey, msgModel);
        _subResults.Add(res);
        return res;
    }
}

public class Message
{
    public Message(string key, object? model = null)
    {
        Key = key;
        Model = model;
    }

    public string Key { get; set; }
    public object? Model { get; set; }
}

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
