using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Common.CliLib;

public static class Io
{
    private static readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    public static string GetSensitiveValue(string promptText)
    {
        Console.Write(promptText);
        var val = string.Empty;
        ConsoleKey key;
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && val.Length > 0)
            {
                Console.Write("\b \b");
                val = val[0..^1];
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                Console.Write("*");
                val += keyInfo.KeyChar;
            }
        } while (key != ConsoleKey.Enter);

        Console.WriteLine();
        return val;
    }

    public static void WriteYml(dynamic any)
    {
        Console.Write(_serializer.Serialize(any));
    }
}
