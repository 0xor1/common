﻿using System.Text.RegularExpressions;
using Common.Shared;

namespace Common.Shared;

public record ValidationResult
{
    public bool Valid { get; set; } = true;
    public Message Message { get; set; } = new(S.Invalid);
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
    public static ValidationResult Email(string str)
    {
        var res = new ValidationResult() { Message = new(S.AuthInvalidEmail) };
        if (!EmailRegex().IsMatch(str))
        {
            res.Valid = false;
        }
        return res;
    }

    public static ValidationResult Pwd(string str)
    {
        var res = new ValidationResult() { Message = new(S.AuthInvalidPwd) };
        if (!EightOrMoreCharsRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(S.AuthLessThan8Chars));
        }
        if (!LowerCaseRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(S.AuthNoLowerCaseChar));
        }
        if (!UpperCaseRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(S.AuthNoUpperCaseChar));
        }
        if (!DigitRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(S.AuthNoDigit));
        }
        if (!SpecialCharRegex().IsMatch(str))
        {
            res.Valid = false;
            res.SubMessages.Add(new(S.AuthNoSpecialChar));
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
