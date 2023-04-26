using Humanizer;

namespace Common.Shared;

public static class EnumExt
{
    public static Key ToKey(this Enum input)
    {
        return Key.Force(input.Humanize(LetterCasing.Title));
    }
}
