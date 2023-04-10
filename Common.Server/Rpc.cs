using System.Net;
using Common.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Server;

public class RpcException : Exception
{
    public HttpStatusCode Code { get; }

    public RpcException(string message, HttpStatusCode code = HttpStatusCode.InternalServerError)
        : base(message)
    {
        Code = code;
    }
}

public interface IRpcEndpoint
{
    Task Execute(HttpContext ctx);
}

public record RpcEndpoint<TArg, TRes>(Rpc<TArg, TRes> Def, Func<HttpContext, TArg, Task<TRes>> Fn)
    : IRpcEndpoint
    where TArg : class
    where TRes : class
{
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

    private async Task WriteResp(HttpContext ctx, TRes res)
    {
        if (Rpc.HasStream<TRes>())
        {
            ctx.Response.Headers.Add(Rpc.DataHeader, Rpc.Serialize(res).ToB64());
            ctx.Response.Body = (res as IStream).NotNull().Stream;
        }
        else
        {
            var bs = Rpc.Serialize(res);
            ctx.Response.ContentLength = bs.Length;
            ctx.Response.Body = new MemoryStream(bs);
        }
    }

    private async Task HandleException(HttpContext ctx, Exception ex)
    {
        var code = 500;
        var message = S.UnexpectedError;
        if (ex is RpcException)
        {
            var re = (ex as RpcException).NotNull();
            code = (int)re.Code;
            message = re.Message;
        }
        else
        {
            ctx.Get<ILogger<RpcEndpoint<TArg, TRes>>>().LogError(ex, $"Error thrown by {Def.Path}");
        }
        ctx.Response.StatusCode = code;
        await ctx.Response.WriteAsync(message);
    }
}
