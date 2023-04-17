using System.Net;
using Common.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Server;

public interface IRpcEndpoint
{
    string Path { get; }
    Task Execute(HttpContext ctx);
}

public record RpcEndpoint<TArg, TRes>(Rpc<TArg, TRes> Def, Func<HttpContext, TArg, Task<TRes>> Fn)
    : IRpcEndpoint
    where TArg : class
    where TRes : class
{
    public string Path => Def.Path;

    public async Task Execute(HttpContext ctx)
    {
        try
        {
            var arg = await GetArg(ctx);
            var res = await Fn(ctx, arg);
            await WriteResp(ctx, res);
        }
        catch (Exception ex)
        {
            await HandleException(ctx, ex);
        }
    }

    private static async Task<TArg> GetArg(HttpContext ctx)
    {
        var argBs = Array.Empty<byte>();
        if (ctx.Request.Query.ContainsKey(Rpc.QueryParam))
        {
            argBs = ctx.Request.Query[Rpc.QueryParam].ToString().FromB64();
        }
        else if (ctx.Request.Headers.ContainsKey(Rpc.DataHeader))
        {
            argBs = ctx.Request.Headers[Rpc.DataHeader].ToString().FromB64();
        }
        else if (!Rpc.HasStream<TArg>())
        {
            using var ms = new MemoryStream();
            await ctx.Request.Body.CopyToAsync(ms);
            argBs = ms.ToArray();
        }

        var arg = Rpc.Deserialize<TArg>(argBs);

        if (Rpc.HasStream<TArg>())
        {
            var sub = (arg as IStream).NotNull();
            sub.Stream = ctx.Request.Body;
        }

        return arg;
    }

    private async Task WriteResp(HttpContext ctx, TRes val)
    {
        IResult res;
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        if (Rpc.HasStream<TRes>())
        {
            res = Results.Stream((val as IStream).NotNull().Stream);
            ctx.Response.Headers.Add(Rpc.DataHeader, Rpc.Serialize(val).ToB64());
        }
        else
        {
            res = Results.Bytes(Rpc.Serialize(val));
        }
        await res.ExecuteAsync(ctx);
    }

    private async Task HandleException(HttpContext ctx, Exception ex)
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
            ctx.Get<ILogger<RpcEndpoint<TArg, TRes>>>().LogError(ex, $"Error thrown by {Def.Path}");
        }

        ctx.Response.StatusCode = code;
        await Results.Text(content: message, statusCode: code).ExecuteAsync(ctx);
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
                        ctx.ErrorIf(
                            !epsDic.TryGetValue(
                                ctx.Request.Path.Value.NotNull().ToLower(),
                                out var ep
                            ),
                            S.RpcUnknownEndpoint,
                            null,
                            HttpStatusCode.NotFound
                        );
                        await ep.NotNull().Execute(ctx);
                    }
                )
        );
        return app;
    }
}
