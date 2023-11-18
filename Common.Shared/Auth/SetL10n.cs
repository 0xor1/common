namespace Common.Shared.Auth;

public record SetL10n(
    string Lang,
    DateFmt DateFmt,
    string TimeFmt,
    string DateSeparator,
    string ThousandsSeparator,
    string DecimalSeparator
);
