using Newtonsoft.Json;

namespace Common.Shared.Auth;

public record Session(
    string Id,
    bool IsAuthed,
    DateTime StartedOn,
    bool RememberMe,
    string Lang,
    string DateFmt,
    string TimeFmt,
    bool FcmEnabled
) : ISession
{
    public static Session Default(string lang, string dateFmt, string timeFmt) =>
        new(string.Empty, false, DateTime.UtcNow, false, lang, dateFmt, timeFmt, false);

    [JsonIgnore]
    public bool IsAnon => !IsAuthed;
}
