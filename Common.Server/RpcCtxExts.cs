using System.Net;
using System.Text;
using Common.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CS = Common.Shared.I18n.S;
using ISession = Common.Shared.Auth.ISession;

namespace Common.Server;

public static class RpcCtxExts
{
    public static T Get<T>(this HttpContext ctx)
        where T : notnull => ctx.RequestServices.GetRequiredService<T>();

    public static T GetFeature<T>(this HttpContext ctx)
        where T : notnull => ctx.Features.GetRequiredFeature<T>();

    public static ISession GetAuthedSession(this IRpcCtx ctx)
    {
        var ses = ctx.GetSession();
        ctx.ErrorIf(ses.IsAnon, CS.AuthNotAuthenticated, null, HttpStatusCode.Unauthorized);
        return ses;
    }

    public static async Task<TRes> DbTx<TDbCtx, TRes>(
        this IRpcCtx ctx,
        Func<TDbCtx, ISession, Task<TRes>> fn,
        bool mustBeAuthedSession = true
    )
        where TDbCtx : DbContext
    {
        var db = ctx.Get<TDbCtx>();
        var tx = await db.Database.BeginTransactionAsync(ctx.Ctkn);
        try
        {
            var res = await fn(db, mustBeAuthedSession ? ctx.GetAuthedSession() : ctx.GetSession());
            await db.SaveChangesAsync(ctx.Ctkn);
            await tx.CommitAsync(ctx.Ctkn);
            return res;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ctx.Ctkn);
            throw;
        }
    }

    public static void ErrorIf(
        this IRpcCtx ctx,
        bool condition,
        string key,
        object? model = null,
        HttpStatusCode code = HttpStatusCode.InternalServerError
    )
    {
        Throw.If(condition, () => new RpcException(ctx.String(key, model), (int)code));
    }

    public static void NotFoundIf(
        this IRpcCtx ctx,
        bool condition,
        string? key = null,
        object? model = null
    )
    {
        ctx.ErrorIf(condition, key ?? CS.EntityNotFound, model, HttpStatusCode.NotFound);
    }

    public static void InsufficientPermissionsIf(
        this IRpcCtx ctx,
        bool condition,
        string? key = null,
        object? model = null
    )
    {
        ctx.ErrorIf(condition, key ?? CS.InsufficientPermission, model, HttpStatusCode.Forbidden);
    }

    public static void BadRequestIf(
        this IRpcCtx ctx,
        bool condition,
        string? key = null,
        object? model = null
    )
    {
        ctx.ErrorIf(condition, key ?? CS.BadRequest, model, HttpStatusCode.BadRequest);
    }

    public static void ErrorFromValidationResult(
        this IRpcCtx ctx,
        ValidationResult res,
        HttpStatusCode code = HttpStatusCode.InternalServerError
    )
    {
        if (res.Valid)
            return;
        var sb = new StringBuilder();
        BuildValidationStringRecursively(ctx, res, sb);
        throw new RpcException(sb.ToString(), (int)code);
    }

    private static void BuildValidationStringRecursively(
        IRpcCtx ctx,
        ValidationResult res,
        StringBuilder sb,
        int depth = 0
    )
    {
        if (res.Valid)
            return;
        if (depth > 0)
        {
            sb.Append(new String(' ', depth * 2));
        }
        sb.Append(ctx.String(res.Message.Key, res.Message.Model));
        if (res.SubResults.Any(x => !x.Valid))
        {
            sb.Append(':');
        }
        foreach (var subRes in res.SubResults.Where(x => !x.Valid))
        {
            sb.Append('\n');
            BuildValidationStringRecursively(ctx, subRes, sb, depth + 1);
        }
    }

    public static string String(this IRpcCtx ctx, string key, object? model = null)
    {
        return ctx.Get<S>().GetOrAddress(ctx.GetSession().Lang, key, model);
    }
}
