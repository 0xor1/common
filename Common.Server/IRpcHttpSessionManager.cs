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
        string dateFmt,
        string timeFmt,
        bool fcmEnabled
    );

    internal Session Clear(HttpContext ctx);
}
