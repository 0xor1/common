using System.Net;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Server;

public interface IRpcEndpoint
{
    string Path { get; }
    Task Execute(IRpcCtxInternal ctx);
}

public interface IRpcCtx
{
    public T Get<T>()
        where T : notnull;

    public Session GetSession();

    public Session CreateSession(
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        string dateFmt,
        string timeFmt
    );
    public Session ClearSession();
}

public interface IRpcCtxInternal : IRpcCtx
{
    Task<T> GetArg<T>()
        where T : class;
    Task WriteResp<T>(T val)
        where T : class;
    Task HandleException(Exception ex, string path);
}

public class RpcHttpCtx : IRpcCtxInternal
{
    private readonly HttpContext _ctx;
    private readonly IRpcHttpSessionManager _sessionManager;
    private Session? _session;

    public RpcHttpCtx(HttpContext ctx, IRpcHttpSessionManager sessionManager)
    {
        _ctx = ctx;
        _sessionManager = sessionManager;
    }

    public Session GetSession() => _sessionManager.Get(_ctx);

    public Session CreateSession(
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        string dateFmt,
        string timeFmt
    ) => _sessionManager.Create(_ctx, userId, isAuthed, rememberMe, lang, dateFmt, timeFmt);

    public Session ClearSession() => _sessionManager.Clear(_ctx);

    public T Get<T>()
        where T : notnull
    {
        return _ctx.Get<T>();
    }

    public async Task<T> GetArg<T>()
        where T : class
    {
        if (typeof(T) == Nothing.Type)
        {
            return (Nothing.Inst as T).NotNull();
        }
        var argBs = Array.Empty<byte>();
        if (_ctx.Request.Query.ContainsKey(RpcHttp.QueryParam))
        {
            argBs = _ctx.Request.Query[RpcHttp.QueryParam].ToString().FromB64();
        }
        else if (_ctx.Request.Headers.ContainsKey(RpcHttp.DataHeader))
        {
            argBs = _ctx.Request.Headers[RpcHttp.DataHeader].ToString().FromB64();
        }
        else if (!RpcHttp.HasStream<T>())
        {
            using var ms = new MemoryStream();
            await _ctx.Request.Body.CopyToAsync(ms);
            argBs = ms.ToArray();
        }

        var arg = RpcHttp.Deserialize<T>(argBs);

        if (RpcHttp.HasStream<T>())
        {
            var sub = (arg as HasStream).NotNull();
            var hs = _ctx.Request.Headers;
            sub.Stream = new RpcStream(
                _ctx.Request.Body,
                hs[RpcHttp.ContentNameHeader].ToString(),
                hs.ContentType.ToString(),
                false,
                (ulong)hs.ContentLength
            );
        }

        return arg;
    }

    public async Task WriteResp<T>(T val)
        where T : class
    {
        IResult res;
        _ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        if (RpcHttp.HasStream<T>())
        {
            var stream = (val as HasStream).NotNull().Stream;
            res = Results.Stream(stream.Data, stream.Type, stream.IsDownload ? stream.Name : null);
            _ctx.Response.Headers.Add(RpcHttp.DataHeader, RpcHttp.Serialize(val).ToB64());
        }
        else
        {
            if (typeof(T) == Nothing.Type)
            {
                res = Results.StatusCode((int)HttpStatusCode.OK);
            }
            else
            {
                res = Results.Bytes(RpcHttp.Serialize(val));
            }
        }

        await res.ExecuteAsync(_ctx);
    }

    public async Task HandleException(Exception ex, string path)
    {
        var code = 500;
        var message = S.UnexpectedError;
        if (ex is RpcException)
        {
            var re = (ex as RpcException).NotNull();
            code = re.Code;
            message = re.Message;
        }
        else
        {
            Get<ILogger<IRpcCtx>>().LogError(ex, $"Error thrown by {path}");
        }

        _ctx.Response.StatusCode = code;
        await Results.Text(content: message, statusCode: code).ExecuteAsync(_ctx);
    }
}

public record RpcEndpoint<TArg, TRes>(Rpc<TArg, TRes> Def, Func<IRpcCtx, TArg, Task<TRes>> Fn)
    : IRpcEndpoint
    where TArg : class
    where TRes : class
{
    public string Path => Def.Path;

    public async Task Execute(IRpcCtxInternal ctx)
    {
        try
        {
            var arg = await ctx.GetArg<TArg>();
            var res = await Fn(ctx, arg);
            await ctx.WriteResp(res);
        }
        catch (Exception ex)
        {
            await ctx.HandleException(ex, Def.Path);
        }
    }
}

public static class RpcExts
{
    public static IApplicationBuilder UseRpcEndpoints(
        this IApplicationBuilder app,
        IReadOnlyList<IRpcEndpoint> eps
    )
    {
        // validate all endpoints start /api/ and there are no duplicates
        var dupedPaths = eps.Select(x => x.Path).GetDuplicates().ToList();
        Throw.SetupIf(
            dupedPaths.Any(),
            $"Some rpc endpoints have duplicate paths {string.Join(",", dupedPaths)}"
        );
        var epsDic = eps.ToDictionary(x => x.Path).AsReadOnly();
        app.Map(
            "/api",
            app =>
                app.Run(
                    async (ctx) =>
                    {
                        var rpcCtx = new RpcHttpCtx(ctx, ctx.Get<IRpcHttpSessionManager>());
                        rpcCtx.ErrorIf(
                            !epsDic.TryGetValue(
                                ctx.Request.Path.Value.NotNull().ToLower(),
                                out var ep
                            ),
                            S.RpcUnknownEndpoint,
                            null,
                            HttpStatusCode.NotFound
                        );
                        await ep.NotNull().Execute(rpcCtx);
                    }
                )
        );
        return app;
    }
}

public static class RpcCtxExts
{
    public static T Get<T>(this HttpContext ctx)
        where T : notnull => ctx.RequestServices.GetRequiredService<T>();

    public static Session GetAuthedSession(this IRpcCtx ctx)
    {
        var ses = ctx.GetSession();
        ctx.ErrorIf(ses.IsAnon, S.AuthNotAuthenticated, null, HttpStatusCode.Unauthorized);
        return ses;
    }

    public static async Task<TRes> DbTx<TDbCtx, TRes>(
        this IRpcCtx ctx,
        Func<TDbCtx, Session, Task<TRes>> fn,
        bool mustBeAuthedSession = true
    )
        where TDbCtx : DbContext
    {
        var db = ctx.Get<TDbCtx>();
        var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var res = await fn(db, mustBeAuthedSession ? ctx.GetAuthedSession() : ctx.GetSession());
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

    public static void ErrorFromValidationResult(
        this IRpcCtx ctx,
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

    public static string String(this IRpcCtx ctx, string key, object? model = null)
    {
        return ctx.Get<S>().GetOrAddress(ctx.GetSession().Lang, key, model);
    }
}
