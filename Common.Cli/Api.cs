using Cocona;
using Common.Shared;
using Fluid;
using Fluid.Values;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Common.Cli;

public class Api
{
    private const string ApiFile = 
        """
        // Generated Code File, Do Not Edit.
        // This file is generated with Common.Cli.
        // see https://github.com/0xor1/common/blob/main/Common.Cli/Api.cs
        // executed with arguments: api {{YmlDirPath}}

        using Common.Shared;
        using Common.Shared.Auth;
        {% for sec in Sections %}using {{ApiNameSpace}}.{{sec.Key}};
        {% endfor %}
        
        namespace {{ApiNameSpace}};
        
        public interface IApi : Common.Shared.Auth.IApi
        {
            {% for sec in Sections %}public I{{sec.Key}}Api {{sec.Key}} { get; }
            {% endfor %}
        }
        
        public class Api : IApi
        {
            public Api(IRpcClient client)
            {
                App = new AppApi(client);
                Auth = new AuthApi(client);
                {% for sec in Sections %}{{sec.Key}} = new {{sec.Key}}Api(client);
                {% endfor %}
            }
        
            public IAppApi App { get; }
            public IAuthApi Auth { get; }
            {% for sec in Sections %}public I{{sec.Key}}Api {{sec.Key}} { get; }
            {% endfor %}
        }
        """;

    private const string SectionFile = 
        """
        // Generated Code File, Do Not Edit.
        // This file is generated with Common.Cli.
        // see https://github.com/0xor1/common/blob/main/Common.Cli/Api.cs
        // executed with arguments: api {{YmlDirPath}}

        #nullable enable
        
        using Common.Shared;
        using MessagePack;
        {% for import in Imports %}using {{import}};
        {% endfor %}
        
        namespace {{ApiNameSpace}}.{{Key}};
        
        public interface I{{Key}}Api
        {
            {% for ep in Eps %}public {% if ep.FullyQualifyTask %}System.Threading.Tasks.{% endif %}Task{% if ep.Res != "Nothing" %}<{{ep.Res}}>{% endif %} {{ep.Key}}({% if ep.Arg != "Nothing" %}{{ep.Arg}} arg, {% endif %}CancellationToken ctkn = default);{% if ep.GetUrl %}
            public string {{ep.Key}}Url({% if ep.Arg != "Nothing" %}{{ep.Arg}} arg{% endif %});{% endif %}
            {% endfor %}
        }
        
        public class {{Key}}Api : I{{Key}}Api
        {
            private readonly IRpcClient _client;
        
            public {{Key}}Api(IRpcClient client)
            {
                _client = client;
            }
        
            {% for ep in Eps %}public {% if ep.FullyQualifyTask %}System.Threading.Tasks.{% endif %}Task{% if ep.Res != "Nothing" %}<{{ep.Res}}>{% endif %} {{ep.Key}}({% if ep.Arg != "Nothing" %}{{ep.Arg}} arg, {% endif %}CancellationToken ctkn = default) =>
                _client.Do({{Key}}Rpcs.{{ep.Key}}, {% if ep.Arg != "Nothing" %}arg{% else %}Nothing.Inst{% endif %}, ctkn);
            {% if ep.GetUrl %}
            public string {{ep.Key}}Url({% if ep.Arg != "Nothing" %}{{ep.Arg}} arg{% endif %}) =>
                _client.GetUrl({{Key}}Rpcs.{{ep.Key}}, {% if ep.Arg != "Nothing" %}arg{% else %}Nothing.Inst{% endif %});
            {% endif %}
            {% endfor %}
        }
        
        public static class {{Key}}Rpcs
        {
            {% for ep in Eps %}public static readonly Rpc<{{ep.Arg}}, {{ep.Res}}> {{ep.Key}} = new("/{{Key | lowerfirst }}/{{ep.Key | lowerfirst}}");
            {% endfor %}
        }
        
        {% for type in Types  %}
        {% if type.IsInterface %}
        public interface {{type.Key}}{% if type.Extends %} : {{type.Extends}}{% endif %}
        {
            {% for field in type.Fields %}[Key({{forloop.index0}})]
            public {{field.Type}} {{field.Key}} { get; set; }{% if field.Default %} = {{field.Default}};{% endif %}
            {% endfor %}
        }
        {% else %}
        [MessagePackObject]
        public record {{type.Key}}{% if type.Extends %} : {{type.Extends}}{% endif %}
        {
            public {{type.Key}}(
                {% for field in type.Fields %}{{field.Type}} {{field.Key | lowerfirst }}{% if field.Default %} = {{field.Default}}{% endif %}{% unless forloop.last %},{% endunless %}
                {% endfor %}
            )
            {
                {% for field in type.Fields %}{{field.Key}} = {{field.Key | lowerfirst }};
                {% endfor %}
            }
            
            {% for field in type.Fields %}[Key({{forloop.index0}})]
            public {{field.Type}} {{field.Key}} { get; set; }{% if field.Default %} = {{field.Default}};{% endif %}
            {% endfor %}
        }
        {% endif %}
        {% endfor %}
        
        {% for enum in Enums  %}
        public enum {{enum.Key}}
        {
            {% for val in enum.Vals  %}{{val}}{% unless forloop.last %},{% endunless %}
            {% endfor %}
        }
        {% endfor %}
        """;

    private const string ymlFileName = "api.yml";
    private const string ApiFileName = "Api.g.cs";

    [Command("api")]
    public async Task Run([Argument] string ymlDirPath)
    {
        var ymlPathSegs = ymlDirPath.Split(['/', '\\']);
        var @namespace = ymlPathSegs.Last();
        var slnDirPath = ymlPathSegs.Take(ymlPathSegs.Length - 1);
        var printCsvDirPath = $"<abs_file_path_to>/{@namespace}";
        using var reader = new StreamReader(Path.Join(ymlDirPath, ymlFileName));
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
            .Build();
        var apiDef = deserializer.Deserialize<ApiDef>(reader);
        apiDef.Validate();
        
        apiDef.ApiNameSpace = @namespace;
        apiDef.YmlDirPath = printCsvDirPath;
        apiDef.Sections.ForEach(x =>
        {
            x.YmlDirPath = printCsvDirPath;
            x.ApiNameSpace = @namespace;
        });
        
        var options = new TemplateOptions();
        options.Filters.AddFilter("lowerfirst", (input, arguments, context) =>
        {
            var source = input.ToStringValue().ToCharArray();
            if (source.Length > 0)
            {
                source[0] = char.ToLower(source[0]);
            }
            return new StringValue(new string(source));
        });
        options.MemberAccessStrategy.Register<ApiDef>();
        options.MemberAccessStrategy.Register<Section>();
        options.MemberAccessStrategy.Register<Type>();
        options.MemberAccessStrategy.Register<Enum>();
        options.MemberAccessStrategy.Register<Field>();
        options.MemberAccessStrategy.Register<Ep>();
        
        var fParser = new FluidParser();
        var apiFileTpl = fParser.Parse(ApiFile).NotNull();
        var sectionFileTpl = fParser.Parse(SectionFile).NotNull();

        await File.WriteAllTextAsync(Path.Join(ymlDirPath, ApiFileName), apiFileTpl.Render(new TemplateContext(apiDef, options)));
        foreach (var s in apiDef.Sections)
        {
            await File.WriteAllTextAsync(Path.Join(ymlDirPath, Path.Join(s.Key, $"{s.Key}.g.cs")), sectionFileTpl.Render(new TemplateContext(s, options)));
        }
    }
}

public class ApiDef
{
    private readonly List<string> _reservedSectionKeys = new (){ "app", "auth" };

    public string YmlDirPath { get; set; }
    public string ApiNameSpace { get; set; }
    public List<Section> Sections { get; set; } = new();

    public void Validate()
    {
        var errors = new List<string>();
        var dupes = Sections.Select(x => x.Key).GetDuplicates();
        if (dupes.Any())
        {
            errors.Add($"duplicate section keys: {dupes.Join()}");
        }

        if (Sections.Any(x => _reservedSectionKeys.Contains(x.Key)))
        {
            errors.Add($"reserved section keys: {_reservedSectionKeys.Join()}");
        }
        
        Sections.ForEach(x => x.Validate(errors));

        if (errors.Any())
        {
            throw new InvalidDataException(string.Join("\n", errors));
        }
    }
}
public class Section
{
    private readonly List<string> _reservedTypeKeys = new (){"nothing"};

    public string YmlDirPath { get; set; }
    public string ApiNameSpace { get; set; }
    public List<string> Imports { get; set; } = new();
    public string Key { get; set; }
    public List<Type> Types { get; set; } = new();
    public List<Type> Interfaces { get; set; } = new();
    public List<Enum> Enums { get; set; } = new();
    public List<Ep> Eps { get; set; } = new();

    public void Validate(List<string> errors)
    {
        var dupes = Types.Select(x => x.Key).GetDuplicates();
        if (dupes.Any())
        {
            errors.Add($"section: {Key}, duplicate type keys: {dupes.Join()}");
        }
        dupes = Enums.Select(x => x.Key).GetDuplicates();
        if (dupes.Any())
        {
            errors.Add($"section: {Key}, duplicate enums keys: {dupes.Join()}");
        }
        dupes = Eps.Select(x => x.Key).GetDuplicates();
        if (dupes.Any())
        {
            errors.Add($"section: {Key}, duplicate eps keys: {dupes.Join()}");
        }
        if (Types.Any(x => _reservedTypeKeys.Contains(x.Key)))
        {
            errors.Add($"section: {Key}, reserved type key used: {Key}, reserved keys: {_reservedTypeKeys.Join()}");
        }
    }
}

public class Type
{
    public string Extends { get; set; }

    public bool IsInterface { get; set; }
    public string Key { get; set; }
    public List<Field> Fields { get; set; } = new();
    
    public void Validate(List<string> errors)
    {
        var dupes = Fields.Select(x => x.Key).GetDuplicates();
        if (dupes.Any())
        {
            errors.Add($"type: {Key}, duplicate field keys: {dupes.Join()}");
        }
    }
}

public class Enum
{
    public string Key { get; set; }
    public List<string> Vals { get; set; } = new();
    
    public void Validate(List<string> errors)
    {
        var dupes = Vals.GetDuplicates();
        if (dupes.Any())
        {
            errors.Add($"enum: {Key}, duplicate vals: {dupes.Join()}");
        }
    }
}

public class Field
{
    public string Key { get; set; }
    public string Type { get; set; }
    public string Default { get; set; }
}

public class Ep
{
    public string Key { get; set; }
    public string Arg { get; set; }
    public string Res { get; set; }
    public bool GetUrl { get; set; }
    public bool FullyQualifyTask { get; set; } = false;
}