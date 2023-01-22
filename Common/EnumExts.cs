
using Humanizer;

namespace Common;

public static class EnumExts
{
    public static Key ToKey(this Enum input)
        => Key.Force(input.Humanize(LetterCasing.Title));
}