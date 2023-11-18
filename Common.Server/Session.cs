using Common.Shared;
using MessagePack;

namespace Common.Server;

[MessagePackObject]
public record Session
{
    [Key(0)]
    public string Id { get; init; }

    [Key(1)]
    public bool IsAuthed { get; init; }

    [Key(2)]
    public DateTime StartedOn { get; init; }

    [Key(3)]
    public bool RememberMe { get; init; }

    [Key(4)]
    public string Lang { get; init; }

    [Key(5)]
    public DateFmt DateFmt { get; init; }

    [Key(6)]
    public string TimeFmt { get; init; }

    [Key(7)]
    public string DateSeparator { get; init; }

    [Key(8)]
    public string ThousandsSeparator { get; init; }

    [Key(9)]
    public string DecimalSeparator { get; init; }

    [Key(10)]
    public bool FcmEnabled { get; init; }

    [IgnoreMember]
    public bool IsAnon => !IsAuthed;

    public Shared.Auth.Session ToApi()
    {
        return new(
            Id,
            IsAuthed,
            StartedOn,
            RememberMe,
            Lang,
            DateFmt,
            TimeFmt,
            DateSeparator,
            ThousandsSeparator,
            DecimalSeparator,
            FcmEnabled
        );
    }
}
