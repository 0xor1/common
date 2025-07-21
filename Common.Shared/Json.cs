using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Shared;

public static class Json
{
    public static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Formatting = Formatting.None,
        MissingMemberHandling = MissingMemberHandling.Error,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = new List<JsonConverter>()
        {
            new StringEnumConverter(),
            new StrTrimConverter(),
        },
    };

    public static string From(object v) => JsonConvert.SerializeObject(v, SerializerSettings);

    public static T To<T>(string s)
        where T : class => JsonConvert.DeserializeObject<T>(s, SerializerSettings).NotNull();
}
