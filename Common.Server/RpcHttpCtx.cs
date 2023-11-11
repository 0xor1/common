using System.Net;
using Common.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Common.Server;

public class RpcHttpCtx : IRpcCtxInternal
{
    private readonly HttpContext _ctx;
    private readonly IRpcHttpSessionManager _sessionManager;

    public RpcHttpCtx(HttpContext ctx)
    {
        _ctx = ctx;
        _sessionManager = ctx.Get<IRpcHttpSessionManager>();
    }

    public CancellationToken Ctkn => _ctx.RequestAborted;

    public Session GetSession() => _sessionManager.Get(_ctx);

    public Session CreateSession(
        string userId,
        bool isAuthed,
        bool rememberMe,
        string lang,
        string dateFmt,
        string timeFmt,
        string thousandsSeparator,
        string decimalSeparator,
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
            thousandsSeparator,
            decimalSeparator,
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

    public T GetFeature<T>()
        where T : notnull
    {
        return _ctx.GetFeature<T>();
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
            await _ctx.Request.Body.CopyToAsync(ms, Ctkn);
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
                (ulong)(hs.ContentLength ?? 0)
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
