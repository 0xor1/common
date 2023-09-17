using System.Text.RegularExpressions;

namespace Common.Shared;

public record ValidationResult
{
    public ValidationResult(string msgKey = S.Invalid, object? msgModel = null)
    {
        Valid = true;
        Message = new Message(msgKey, msgModel);
        SubMessages = new List<Message>();
    }

    public bool Valid { get; private set; }
    public Message Message { get; private set; }
    public List<Message> SubMessages { get; }

    public void InvalidIf(bool condition, string? msgKey = null, object? msgModel = null)
    {
        if (!condition)
            return;
        Valid = false;
        if (!msgKey.IsNullOrEmpty())
        {
            SubMessages.Add(new(msgKey, msgModel));
        }
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
        var res = new ValidationResult(S.AuthInvalidEmail);
        res.InvalidIf(!EmailRegex().IsMatch(str));
        return res;
    }

    public static ValidationResult Pwd(string str)
    {
        var res = new ValidationResult(S.AuthInvalidPwd);
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
