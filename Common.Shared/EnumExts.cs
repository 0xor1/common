
using Humanizer;

namespace Common.Shared;

public static class EnumExts
{
    public static Key ToKey(this Enum input)
        => Key.Force(input.Humanize(LetterCasing.Title));
}