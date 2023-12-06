using Common.Shared;
using Microsoft.AspNetCore.Http;
using ISession = Common.Shared.Auth.ISession;

namespace Common.Server;

public interface IRpcHttpSessionManager
{
    internal ISession Get(HttpContext ctx);

    internal ISession Create(
        HttpContext ctx,
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

    internal ISession Clear(HttpContext ctx);
}
