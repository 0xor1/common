using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Shared;

public static class Json
{
    public static readonly JsonSerializerSettings SerializerSettings =
        new()
        {
            Formatting = Formatting.None,
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter(),
                new StrTrimConverter()
            }
        };

    public static string From(object v) => JsonConvert.SerializeObject(v, SerializerSettings);

    public static T To<T>(string s)
        where T : class => JsonConvert.DeserializeObject<T>(s, SerializerSettings).NotNull();
}

public class StrTrimConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override bool CanConvert(Type objectType) => objectType == typeof(string);

    public override object? ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer
    ) => ((string?)reader.Value)?.Trim();

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        value = ((string?)value)?.Trim();
        writer.WriteValue(value);
    }
}
