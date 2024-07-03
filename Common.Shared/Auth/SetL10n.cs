using MessagePack;

namespace Common.Shared.Auth;

[MessagePackObject]
public record SetL10n(
    [property: Key(0)] string Lang,
    [property: Key(1)] DateFmt DateFmt,
    [property: Key(2)] string TimeFmt,
    [property: Key(3)] string DateSeparator,
    [property: Key(4)] string ThousandsSeparator,
    [property: Key(5)] string DecimalSeparator
);
