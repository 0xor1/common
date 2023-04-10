using MessagePack;
using Newtonsoft.Json;

namespace Common.Shared.Auth;

public static class AuthApi
{
    public static readonly Rpc<Nothing, Session> GetSession = new("/auth/get_session");
    public static Rpc<RegisterReq, Nothing> Register { get; } = new("/auth/register");
}

public record Session(
    string Id,
    bool IsAuthed,
    DateTime StartedOn,
    bool RememberMe,
    string Lang,
    string DateFmt,
    string TimeFmt
)
{
    [JsonIgnore]
    public bool IsAnon => !IsAuthed;
}

public record RegisterReq(string Email, string Pwd);
