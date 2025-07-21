namespace Common.Shared.Test;

public class STests
{
    const string lang = "en";
    const DateFmt dateFmt = DateFmt.YMD;
    const string timeFmt = "hh:mm";
    const string dateSeparator = "-";
    const string thousandsSeparator = ",";
    const string decimalSeparator = ".";
    static readonly S S = new Strings(
        lang,
        dateFmt,
        timeFmt,
        dateSeparator,
        thousandsSeparator,
        decimalSeparator,
        new List<Lang>() { new(lang, "English"), new("es", "Español") },
        new List<DateTimeFmt>() { new(timeFmt), new("h:mmtt") },
        new List<string>() { dateSeparator, "/", "." },
        new List<string>() { thousandsSeparator, decimalSeparator },
        new List<string>() { decimalSeparator, thousandsSeparator },
        new Dictionary<string, IReadOnlyDictionary<string, TemplatableString>>()
        {
            {
                "en",
                new Dictionary<string, TemplatableString>()
                {
                    { "a", new TemplatableString("en A") },
                    { "b", new TemplatableString("en B") },
                    { "c", new TemplatableString("en C") },
                }
            },
            {
                "es",
                new Dictionary<string, TemplatableString>()
                {
                    { "a", new TemplatableString("es A") },
                    { "b", new TemplatableString("es B") },
                    { "c", new TemplatableString("es C") },
                }
            },
        }
    );

    [Fact]
    public void GetMethods_ShouldReturnCorrectStrings()
    {
        Assert.Equal("en A", S.Get(lang, "a"));
        Assert.True(S.TryGet(lang, "a", out var x));
        Assert.Equal("en A", x);
        Assert.Equal("en A", S.GetOrAddress(lang, "a"));
        Assert.Equal("en:d", S.GetOrAddress(lang, "d"));
        Assert.Equal("es A", S.Get("es", "a"));
        Assert.True(S.TryGet("es", "a", out x));
        Assert.Equal("es A", x);
        Assert.Equal("es A", S.GetOrAddress("es", "a"));
        Assert.Equal("es:d", S.GetOrAddress("es", "d"));
    }

    [Fact]
    public void BestLang_ShouldReturnBestLangOption()
    {
        Assert.Equal("en", S.BestLang("en-US;es-ES"));
        Assert.Equal("es", S.BestLang("es-ES;en-US"));
        Assert.Equal("en", S.BestLang("pt-PT;fr-Fr"));
    }
}
