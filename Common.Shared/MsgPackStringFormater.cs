using MessagePack;
using MessagePack.Formatters;

namespace Common.Shared;

public class MsgPackStringFormater : IMessagePackFormatter<string>
{
    public void Serialize(
        ref MessagePackWriter writer,
        string value,
        MessagePackSerializerOptions options
    )
    {
        if (value.IsNull())
        {
            writer.WriteNil();
        }
        else
        {
            value = value.Trim();
            writer.WriteString(value.ToUtf8Bytes());
        }
    }

    public string Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
        reader.ReadString()?.Trim();
}

public class MsgPackStringResolver : IFormatterResolver
{
    public static readonly MsgPackStringResolver Instance = new();

    private static readonly MsgPackStringFormater Formatter = new();

    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        if (typeof(T) == typeof(string))
            return (IMessagePackFormatter<T>)Formatter;
        return null;
    }
}
