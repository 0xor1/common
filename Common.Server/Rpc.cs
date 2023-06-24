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
        string timeFmt,
        bool fcmEnabled
    );
    public Session ClearSession();

    public string? GetHeader(string name);
}

public interface IRpcCtxInternal : IRpcCtx
{
    Task<T> GetArg<T>()
        where T : class;
    Task WriteResp<T>(T val)
        where T : class;
    Task HandleException(Exception ex, string message, int code);
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
        string timeFmt,
        bool fcmEnabled
    ) =>
        _sessionManager.Create(
            _ctx,
            userId,
            isAuthed,
            rememberMe,
            lang,
            dateFmt,
            timeFmt,
            fcmEnabled
        );

    public Session ClearSession() => _sessionManager.Clear(_ctx);

    public string? GetHeader(string name) =>
        _ctx.Request.Headers.ContainsKey(name) ? _ctx.Request.Headers[name].ToString() : null;

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

    public async Task HandleException(Exception ex, string message, int code)
    {
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
            var code = 500;
            var message = ctx.String(S.UnexpectedError);
            if (ex is ArgumentValidationException avex)
            {
                code = (int)HttpStatusCode.BadRequest;
                message = avex switch
                {
                    NullMinMaxValuesException _ => ctx.String(S.MinMaxNullArgs),
                    ReversedMinMaxValuesException rmmve
                        => ctx.String(S.MinMaxReversedArgs, new { rmmve.Min, rmmve.Max }),
                };
            }
            else if (ex is RpcException rex)
            {
                code = rex.Code;
                message = rex.Message;
            }
            else
            {
                ctx.Get<ILogger<IRpcCtx>>().LogError(ex, $"Error thrown by {Def.Path}");
            }
            await ctx.HandleException(ex, message, code);
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

    public static IApplicationBuilder UseRpcHost(this IApplicationBuilder app, string rpcHost)
    {
        var baseHref = rpcHost + "/api";
        // validate all endpoints start /api/ and there are no duplicates
        app.Map(
            "/api",
            app =>
                app.Run(
                    async (ctx) =>
                    {
                        var cl = ctx.RequestServices
                            .GetRequiredService<IHttpClientFactory>()
                            .CreateClient();
                        var req = CreateProxyHttpRequest(ctx, baseHref);
                        var res = await cl.SendAsync(req);
                        await CopyProxyHttpResponse(ctx, res);
                    }
                )
        );
        return app;
    }

    private static HttpRequestMessage CreateProxyHttpRequest(
        this HttpContext context,
        string baseHref
    )
    {
        var req = context.Request;

        var reqMsg = new HttpRequestMessage();
        var reqMethod = req.Method;
        reqMsg.Method = new HttpMethod(reqMethod);
        reqMsg.Content = new StreamContent(req.Body);

        foreach (var header in req.Headers)
        {
            if (
                !reqMsg.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray())
                && reqMsg.Content != null
            )
            {
                reqMsg.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        reqMsg.RequestUri = new Uri($"{baseHref}{req.Path}{req.QueryString}");
        return reqMsg;
    }

    private static async Task CopyProxyHttpResponse(
        this HttpContext context,
        HttpResponseMessage respMsg
    )
    {
        if (respMsg == null)
        {
            throw new ArgumentNullException(nameof(respMsg));
        }

        var resp = context.Response;

        resp.StatusCode = (int)respMsg.StatusCode;
        foreach (var header in respMsg.Headers)
        {
            resp.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in respMsg.Content.Headers)
        {
            resp.Headers[header.Key] = header.Value.ToArray();
        }

        await using var responseStream = await respMsg.Content.ReadAsStreamAsync();
        await responseStream.CopyToAsync(resp.Body, context.RequestAborted);
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

    public static void NotFoundIf(
        this IRpcCtx ctx,
        bool condition,
        string? key = null,
        object? model = null
    )
    {
        ctx.ErrorIf(condition, key ?? S.EntityNotFound, model, HttpStatusCode.NotFound);
    }

    public static void InsufficientPermissionsIf(
        this IRpcCtx ctx,
        bool condition,
        string? key = null,
        object? model = null
    )
    {
        ctx.ErrorIf(condition, key ?? S.InsufficientPermission, model, HttpStatusCode.Forbidden);
    }

    public static void BadRequestIf(
        this IRpcCtx ctx,
        bool condition,
        string? key = null,
        object? model = null
    )
    {
        ctx.ErrorIf(condition, key ?? S.BadRequest, model, HttpStatusCode.BadRequest);
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
