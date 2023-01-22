using System.Text.RegularExpressions;

namespace Common;

public class ValidationResult
{
    public bool Valid { get; set; } = true;
    public string Message { get; set; } = "Invalid";
    public List<string> SubMessages { get; set; } = new();
}
public static class AuthValidator
{
    public static ValidationResult Email(string str)
    {
        var res = new ValidationResult()
        {
            Message = "Invalid email"
        };
        if (!Regex.IsMatch(str, @"^[^@]+@[^@]+\.[^@]+$"))
        {
            res.Valid = false;
        }
        return res;
    }

    public static ValidationResult Pwd(string str)
    {
        var res = new ValidationResult()
        {
            Message = "Invalid Password"
        };
        if (!Regex.IsMatch(str, ".{8,}"))
        {
            res.Valid = false;
            res.SubMessages.Add("less than 8 characters");
        }
        if (!Regex.IsMatch(str, "[a-z]"))
        {
            res.Valid = false;
            res.SubMessages.Add("no lowercase character");
        }
        if (!Regex.IsMatch(str, "[A-Z]"))
        {
            res.Valid = false;
            res.SubMessages.Add("no uppercase character");
        }
        if (!Regex.IsMatch(str, "[0-9]"))
        {
            res.Valid = false;
            res.SubMessages.Add("no digit");
        }
        if (!Regex.IsMatch(str, "[^a-zA-Z0-9 ]"))
        {
            res.Valid = false;
            res.SubMessages.Add("no special character");
        }
        return res;
    }
}