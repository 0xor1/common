using System.Text.RegularExpressions;

namespace Common.Shared;

public record ValidationResult
{
    public bool Valid { get; set; } = true;
    public Message Message { get; set; } = new(AuthValidator.Strings.Invalid);
    public List<Message> SubMessages { get; } = new();
}

public class Message
{
    public string Key { get; set; }
    public object? Model { get; set; }

    public Message(string key, object? model = null)
    {
        Key = key;
        Model = model;
    }
}

public static partial class AuthValidator
{
    public static class Strings
    {
        public const string Invalid = "invalid";
        public const string InvalidEmail = "invalid_email";
        public const string InvalidPwd = "invalid_pwd";
        public const string LessThan8Chars = "less_than_8_chars";
        public const string NoLowerCaseChar = "no_lower_case_char";
        public const string NoUpperCaseChar = "no_upper_case_char";
        public const string NoDigit = "no_digit";
        public const string NoSpecialChar = "no_special_char";
    }

    public static ValidationResult Email(string str)
    {
        var res = new ValidationResult() { Message = new(Strings.InvalidEmail) };
        if (!EmailRegex().IsMatch(str))
        {
            res.Valid = false;
        }
        return res;
    }

    public static ValidationResult Pwd(string str)
    {
        var res = new ValidationResult() { Message = new(Strings.InvalidPwd) };
        if (!EightOrMoreCharsRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(Strings.LessThan8Chars));
        }
        if (!LowerCaseRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(Strings.NoLowerCaseChar));
        }
        if (!UpperCaseRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(Strings.NoUpperCaseChar));
        }
        if (!DigitRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(Strings.NoDigit));
        }
        if (!SpecialCharRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(Strings.NoSpecialChar));
        }
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
