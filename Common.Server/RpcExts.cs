using System.Net;
using Common.Shared;
using S = Common.Shared.I18n.S;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Server;

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
                        var rpcCtx = new RpcHttpCtx(ctx);
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
                        var res = await cl.SendAsync(req, ctx.RequestAborted);
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
        this HttpContext ctx,
        HttpResponseMessage respMsg
    )
    {
        if (respMsg == null)
        {
            throw new ArgumentNullException(nameof(respMsg));
        }

        var resp = ctx.Response;

        resp.StatusCode = (int)respMsg.StatusCode;
        foreach (var header in respMsg.Headers)
        {
            resp.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in respMsg.Content.Headers)
        {
            resp.Headers[header.Key] = header.Value.ToArray();
        }

        await using var responseStream = await respMsg.Content.ReadAsStreamAsync(
            ctx.RequestAborted
        );
        await responseStream.CopyToAsync(resp.Body, ctx.RequestAborted);
    }
}
