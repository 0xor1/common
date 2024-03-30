using System.Net;
using Common.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ISession = Common.Shared.Auth.ISession;
using S = Common.Shared.I18n.S;

namespace Common.Server;

public record RpcEndpoint<TArg, TRes>(Rpc<TArg, TRes> Def, Func<IRpcCtx, TArg, Task<TRes>> Fn)
    : IRpcEndpoint
    where TArg : class
    where TRes : class
{
    public static RpcEndpoint<TArg, TRes> DbTx<TDb, TArg, TRes>(
        Rpc<TArg, TRes> def,
        Func<IRpcCtx, TDb, ISession, TArg, Task<TRes>> fn,
        bool mustBeAuthedSes = true
    )
        where TDb : DbContext
        where TArg : class
        where TRes : class =>
        new(
            def,
            async (ctx, arg) =>
                await ctx.DbTx<TDb, TRes>(
                    async (db, ses) => await fn(ctx, db, ses, arg),
                    mustBeAuthedSes
                )
        );

    public string Path => Def.Path;
    public ulong? MaxSize => Def.MaxSize;

    public async Task Execute(IRpcCtxInternal ctx)
    {
        try
        {
            ctx.GetFeature<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = (long?)MaxSize;
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
            else if (
                ex is BadHttpRequestException bre
                && bre.Message.Contains("Request body too large")
            )
            {
                code = bre.StatusCode;
                message = ctx.String(S.RequestBodyTooLarge, new { MaxSize });
            }
            else
            {
                ctx.Get<ILogger<IRpcCtx>>().LogError(ex, $"Error thrown by {Def.Path}");
            }
            await ctx.HandleException(ex, message, code);
        }
    }
}
