using Common.Shared;

namespace Common.Server;

public interface IRpcCtx
{
    public T Get<T>()
        where T : notnull;

    public T GetFeature<T>()
        where T : notnull;

    public Session GetSession();

    public Session CreateSession(
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
    public Session ClearSession();

    public string? GetHeader(string name);

    public CancellationToken Ctkn { get; }
}
