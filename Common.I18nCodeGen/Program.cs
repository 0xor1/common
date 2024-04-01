// See https://aka.ms/new-console-template for more information

using System.Globalization;
using Common.Shared;
using CsvHelper;
using Fluid;


const string KeysFile = 
    """
    // Generated Code File, Do Not Edit.
    // This file is generated with Common.I18nCodeGen.
    
    namespace {{Namespace}};

    public static partial class S
    { {% for key in Keys %}
        {{key}}{% endfor %}
    }
    """;

const string LangFile = 
    """
    // Generated Code File, Do Not Edit.
    // This file is generated with Common.I18nCodeGen.
    
    using Common.Shared;
    
    namespace {{Namespace}};

    public static partial class S
    {
        private static readonly {%if ReadOnly %}IReadOnly{% endif %}Dictionary<string, TemplatableString> {{Lang}}_Strings = new Dictionary<string, TemplatableString>()
        { {% for str in Strings %}
            {{str}}{% endfor %}
        };
    }
    """;

var csvDirPath = args[0];
var @namespace = args[1];
var @readonly = bool.Parse(args[2]);
var prefix = "";
if (args.Length == 4)
{
    prefix = args[3];
}
var fParser = new FluidParser();
var keyFileTpl = fParser.Parse(KeysFile).NotNull();
var langFileTpl = fParser.Parse(LangFile).NotNull();
using (var reader = new StreamReader(Path.Join(csvDirPath, "strings.csv")))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    csv.Read();
    csv.ReadHeader();
    var langs = csv.HeaderRecord.NotNull().TakeLast(csv.HeaderRecord.NotNull().Length - 1).ToList();
    var kfm = new KeyFileModel(@namespace);
    var lfms = new Dictionary<string, LangFileModel>();
    foreach (var lang in langs)
    {
        lfms.Add(lang, new LangFileModel(@namespace, @readonly, lang.ToUpper()));
    }

    while (csv.Read())
    {
        var key = csv.GetField<string>("key").NotNull();
        kfm.Keys.Add(new ContentKey(key, prefix));
        foreach (var lang in langs)
        {
            var content = csv.GetField<string>(lang).NotNull();
            lfms[lang].Strings.Add(new (key, content));
        }
    }
    File.WriteAllText(Path.Join(csvDirPath, "Keys.cs"), keyFileTpl.Render(new TemplateContext(kfm)));
    foreach (var lang in langs)
    {
        File.WriteAllText(Path.Join(csvDirPath, $"S{lang.ToUpper()}.cs"), langFileTpl.Render(new TemplateContext(lfms[lang])));
    }
}
    
    

public record KeyFileModel(string Namespace)
{
    public List<ContentKey> Keys { get; } = new();

}
public record ContentKey(string Key, string prefix)
{
    public string Pascal => new Key(Key).ToPascal();
    
    public override string ToString() => $"public const string {Pascal} = \"{prefix}{Key}\";";
}
public record LangFileModel(string Namespace, bool ReadOnly, string Lang)
{
    public List<StringContent> Strings { get; } = new();

}
public record StringContent(string Key, string Content)
{
    public string Pascal => new Key(Key).ToPascal();
    public override string ToString() => $"{{ {Pascal}, new(\"{Content.Replace("\"", "\\\"").ReplaceLineEndings(@"\n")}\") }},";
}