using System.Net;
using Common.Shared;

namespace Common.CliLib;

[Serializable]
public class State
{
    internal string BaseHref { get; set; } = "";

    internal CookieCollection Cookies { get; set; } = new();
    internal CookieContainer CookieContainer { get; set; } = new();

    internal Dictionary<string, string> Values { get; set; } = new();

    public string? GetString(string key) => Values.GetValueOrDefault(key, null);

    public int? GetInt(string key)
    {
        var val = Values.GetValueOrDefault(key, null);
        if (!val.IsNullOrEmpty() && int.TryParse(val, out int result))
        {
            return result;
        }

        return null;
    }

    public bool? GetBool(string key)
    {
        var val = Values.GetValueOrDefault(key, null);
        if (!val.IsNullOrEmpty() && bool.TryParse(val, out bool result))
        {
            return result;
        }

        return null;
    }

    public float? GetFloat(string key)
    {
        var val = Values.GetValueOrDefault(key, null);
        if (!val.IsNullOrEmpty() && float.TryParse(val, out float result))
        {
            return result;
        }

        return null;
    }

    public double? GetDouble(string key)
    {
        var val = Values.GetValueOrDefault(key, null);
        if (!val.IsNullOrEmpty() && double.TryParse(val, out double result))
        {
            return result;
        }

        return null;
    }

    public decimal? GetDecimal(string key)
    {
        var val = Values.GetValueOrDefault(key, null);
        if (!val.IsNullOrEmpty() && decimal.TryParse(val, out decimal result))
        {
            return result;
        }

        return null;
    }

    public DateTime? GetDateTime(string key)
    {
        var val = Values.GetValueOrDefault(key, null);
        if (!val.IsNullOrEmpty() && DateTime.TryParse(val, out DateTime result))
        {
            return result;
        }

        return null;
    }
}
