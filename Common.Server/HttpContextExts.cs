using System.Net;
using Common.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server;

public static class HttpContextExts
{
    // these require that ISessionManager was added to service container
    public static T Get<T>(this HttpContext ctx)
        where T : notnull
    {
        return ctx.RequestServices.GetRequiredService<T>();
    }

    private static ISessionManager GetSessionManager(this HttpContext ctx)
    {
        return ctx.Get<ISessionManager>();
    }

    public static Session GetSession(this HttpContext ctx)
    {
        return ctx.GetSessionManager().Get(ctx);
    }

    public static Session GetAuthedSession(this HttpContext ctx)
    {
        var ses = ctx.GetSession();
        ctx.ErrorIf(ses.IsAnon, S.AuthNotAuthenticated, null, HttpStatusCode.Unauthorized);
        return ses;
    }

    public static async Task<TRes> DbTx<TDBCtx, TRes>(
        this HttpContext ctx,
        Func<TDBCtx, Session, Task<TRes>> fn
    )
        where TDBCtx : DbContext
    {
        var db = ctx.Get<TDBCtx>();
        var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var res = await fn(db, ctx.GetAuthedSession());
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return res;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public static Session CreateSession(
        this HttpContext ctx,
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        string dateFmt,
        string timeFmt
    )
    {
        return ctx.GetSessionManager()
            .Create(ctx, userId, isAuthed, rememberMe, lang, dateFmt, timeFmt);
    }

    public static Session ClearSession(this HttpContext ctx)
    {
        return ctx.GetSessionManager().Clear(ctx);
    }

    public static void ErrorIf(
        this HttpContext ctx,
        bool condition,
        string key,
        object? model = null,
        HttpStatusCode code = HttpStatusCode.InternalServerError
    )
    {
        Throw.If(condition, () => new RpcException(ctx.String(key, model), (int)code));
    }

    public static void ErrorFromValidationResult(
        this HttpContext ctx,
        ValidationResult res,
        HttpStatusCode code = HttpStatusCode.InternalServerError
    )
    {
        Throw.If(
            !res.Valid,
            () =>
                new RpcException(
                    $"{ctx.String(res.Message.Key, res.Message.Model)}{(res.SubMessages.Any() ? $":\n{string.Join("\n", res.SubMessages.Select(x => ctx.String(x.Key, x.Model)))}" : "")}",
                    (int)code
                )
        );
    }

    public static string String(this HttpContext ctx, string key, object? model = null)
    {
        return ctx.Get<S>().GetOrAddress(ctx.GetSession().Lang, key, model);
    }
}
