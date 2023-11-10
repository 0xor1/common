using Newtonsoft.Json;

namespace Common.Shared;

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
