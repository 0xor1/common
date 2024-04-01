using Common.Shared.Auth;
using Microsoft.EntityFrameworkCore;

namespace Common.Server.Auth;

public class CommonEps<TDbCtx>
    where TDbCtx : DbContext, IAuthDb
{
    private readonly AuthEps<TDbCtx> _authEps;

    public CommonEps(
        int maxAuthAttemptsPerSecond,
        bool fcmRequiresAuth,
        int maxFcmRegs,
        Func<IRpcCtx, TDbCtx, string, string, Task> onActivation,
        Func<IRpcCtx, TDbCtx, ISession, Task> onDelete,
        Func<IRpcCtx, TDbCtx, ISession, IReadOnlyList<string>, Task> validateFcmTopic
    )
    {
        _authEps = new AuthEps<TDbCtx>(
            maxAuthAttemptsPerSecond,
            fcmRequiresAuth,
            maxFcmRegs,
            onActivation,
            onDelete,
            validateFcmTopic
        );
    }

    public IReadOnlyList<IEp> Eps
    {
        get
        {
            var res = AppEps.Eps.ToList();
            res.AddRange(_authEps.Eps);
            return res;
        }
    }
}
