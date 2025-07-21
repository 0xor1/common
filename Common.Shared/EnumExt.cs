using Humanizer;

namespace Common.Shared;

public static class EnumExt
{
    public static Key ToKey<T>(this T input)
        where T : struct, Enum
    {
        return Key.Force(input.Humanize(LetterCasing.Title));
    }
}
