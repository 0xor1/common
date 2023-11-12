using System.Net;
using Common.Shared;
using S = Common.Shared.I18n.S;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace Common.Server;

public record RpcEndpoint<TArg, TRes>(Rpc<TArg, TRes> Def, Func<IRpcCtx, TArg, Task<TRes>> Fn)
    : IRpcEndpoint
    where TArg : class
    where TRes : class
{
    public string Path => Def.Path;
    public long? MaxSize => Def.MaxSize;

    public async Task Execute(IRpcCtxInternal ctx)
    {
        try
        {
            ctx.GetFeature<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = MaxSize;
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
                ex is BadHttpRequestException bre && bre.Message.Contains("Request body too large")
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
