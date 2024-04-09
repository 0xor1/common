using System.Globalization;
using Cocona;
using Common.Shared;
using CsvHelper;
using Fluid;

namespace Common.Cmds;

public class I18n
{
    private const string KeysFile = 
        """
        // Generated Code File, Do Not Edit.
        // This file is generated with Common.Cmds.

        namespace {{Namespace}};

        public static partial class S
        { {% for key in Keys %}
            {{key}}{% endfor %}
        }
        """;

    private const string LangFile = 
        """
        // Generated Code File, Do Not Edit.
        // This file is generated with Common.Cmds.

        using Common.Shared;

        namespace {{Namespace}};

        public static partial class S
        {
            private static readonly {% if ReadOnly %}IReadOnly{% endif %}Dictionary<string, TemplatableString> {{Lang}}_Strings = new {% if ReadOnly %}Dictionary<string, TemplatableString>{% endif %}()
            { {% for str in Strings %}
                {{str}}{% endfor %}
            };
        }
        """;

    private const string ZLibraryFile = 
        """
        // Generated Code File, Do Not Edit.
        // This file is generated with Common.Cmds.

        using Common.Shared;

        namespace {{Namespace}};

        public static partial class S
        {
            private static readonly {%if ReadOnly %}IReadOnly{% endif %}Dictionary<string, {%if ReadOnly %}IReadOnly{% endif %}Dictionary<string, TemplatableString>> Library =
                new {% if ReadOnly %}Dictionary<string, IReadOnlyDictionary<string, TemplatableString>>{% endif %}()
                {
                    {% for lang in Langs %}
                    { Common.Shared.I18n.S.{{lang}}, {{lang}}_Strings },{% endfor %}
                };
        }
        """;

    private const string CsvFileName = "strings.csv";
    private const string Key = "key";
    private const string KeysFileName = "Keys.cs";
    private const string LibraryFileName = "SZLibrary.cs";

    [Command("i18n")]
    public async Task Run([Argument] string csvDirPath, [Argument] string @namespace, [Argument] bool @readonly, [Argument] string prefix = "")
    {
        var fParser = new FluidParser();
        var keyFileTpl = fParser.Parse(KeysFile).NotNull();
        var langFileTpl = fParser.Parse(LangFile).NotNull();
        var zlibFileTpl = fParser.Parse(ZLibraryFile).NotNull();

        using var reader = new StreamReader(Path.Join(csvDirPath, CsvFileName));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await csv.ReadAsync();
        csv.ReadHeader();
        var langs = csv.HeaderRecord.NotNull().TakeLast(csv.HeaderRecord.NotNull().Length - 1).ToList();
        var kfm = new KeyFileModel(@namespace);
        var zlfm = new ZLibraryFileModel(@namespace, @readonly, langs.Select(x => x.ToUpper()).ToList());
        var lfms = new Dictionary<string, LangFileModel>();

        foreach (var lang in langs)
        {
            lfms.Add(lang, new LangFileModel(@namespace, @readonly, lang.ToUpper()));
        }

        while (await csv.ReadAsync())
        {
            var key = csv.GetField<string>(Key).NotNull();
            kfm.Keys.Add(new ContentKey(key, prefix));
            foreach (var lang in langs)
            {
                var content = csv.GetField<string>(lang).NotNull();
                lfms[lang].Strings.Add(new (key, content));
            }
        }
        await File.WriteAllTextAsync(Path.Join(csvDirPath, KeysFileName), keyFileTpl.Render(new TemplateContext(kfm)));
        await File.WriteAllTextAsync(Path.Join(csvDirPath, LibraryFileName), zlibFileTpl.Render(new TemplateContext(zlfm)));
        foreach (var lang in langs)
        {
            await File.WriteAllTextAsync(Path.Join(csvDirPath, $"S{lang.ToUpper()}.cs"), langFileTpl.Render(new TemplateContext(lfms[lang])));
        }
    }

    private record KeyFileModel(string Namespace)
    {
        public List<ContentKey> Keys { get; } = new();

    }

    private record ContentKey(string Key, string Prefix)
    {
        private string Pascal => new Key(Key).ToPascal();
    
        public override string ToString() => $"public const string {Pascal} = \"{Prefix}{Key}\";";
    }

    private record LangFileModel(string Namespace, bool ReadOnly, string Lang)
    {
        public List<StringContent> Strings { get; } = new();

    }

    private record ZLibraryFileModel(string Namespace, bool ReadOnly, List<string> Langs);

    private record StringContent(string Key, string Content)
    {
        private string Pascal => new Key(Key).ToPascal();
        public override string ToString() => $"{{ {Pascal}, new(\"{Content.Replace("\"", "\\\"").ReplaceLineEndings(@"\n")}\") }},";
    }
}