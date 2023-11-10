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
    public string DateFmt { get; init; }

    [Key(6)]
    public string TimeFmt { get; init; }

    [Key(7)]
    public bool FcmEnabled { get; init; }

    [IgnoreMember]
    public bool IsAnon => !IsAuthed;

    public Shared.Auth.Session ToApi()
    {
        return new(Id, IsAuthed, StartedOn, RememberMe, Lang, DateFmt, TimeFmt, FcmEnabled);
    }
}
