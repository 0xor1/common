using Common.Shared;
using Common.Shared.Auth;

namespace Common.Server;

public interface IRpcCtx
{
    public T Get<T>()
        where T : notnull;

    public T GetFeature<T>()
        where T : notnull;

    public ISession GetSession();

    public ISession CreateSession(
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        DateFmt dateFmt,
        string timeFmt,
        string dateSeparator,
        string thousandsSeparator,
        string decimalSeparator,
        bool fcmEnabled
    );
    public ISession ClearSession();

    public string? GetHeader(string name);

    public CancellationToken Ctkn { get; }
}
