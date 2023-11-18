namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<
        string,
        IReadOnlyDictionary<string, TemplatableString>
    > Library = new Dictionary<string, IReadOnlyDictionary<string, TemplatableString>>()
    {
        { EN, EN_Strings },
        { ES, ES_Strings },
        { FR, FR_Strings },
        { DE, DE_Strings },
        { IT, IT_Strings }
    };
}
