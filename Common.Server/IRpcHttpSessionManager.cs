using Common.Shared;
using Microsoft.AspNetCore.Http;

namespace Common.Server;

public interface IRpcHttpSessionManager
{
    internal Session Get(HttpContext ctx);

    internal Session Create(
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

    internal Session Clear(HttpContext ctx);
}
