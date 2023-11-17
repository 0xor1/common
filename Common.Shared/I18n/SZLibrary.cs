namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, TemplatableString>
    > Library = new Dictionary<string, IReadOnlyDictionary<string, TemplatableString>>()
    {
        { EN, English },
        { ES, Spanish },
        { FR, French },
        { DE, German },
        { IT, Italian }
    };
}
