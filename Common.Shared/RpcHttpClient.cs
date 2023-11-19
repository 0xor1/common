using System.Net.Mime;
using Common.Shared.Auth;

namespace Common.Shared;

public record RpcHttpClient : IRpcClient
{
    private static readonly HttpRequestOptionsKey<bool> WebAssemblyEnableStreamingRequestKey =
        new ("WebAssemblyEnableStreamingRequest");
    private readonly string _baseHref;
    private readonly HttpClient _client;
    private readonly Action<string> _rpcExceptionHandler;
    private readonly bool _enableStreaming;

    public RpcHttpClient(string baseHref, HttpClient client, Action<string> reh, bool enableStreaming = false)
    {
        _baseHref = baseHref + "api";
        _client = client;
        _rpcExceptionHandler = reh;
        _enableStreaming = enableStreaming;
    }

    public async Task<TRes> Do<TArg, TRes>(
        Rpc<TArg, TRes> rpc,
        TArg arg,
        CancellationToken ctkn = default
    )
        where TArg : class
        where TRes : class
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, _baseHref + rpc.Path);
        if (RpcHttp.HasStream<TArg>())
        {
            if (_enableStreaming)
            {
                // https://github.com/dotnet/runtime/pull/91295
                // req.SetBrowserRequestStreamingEnabled(true);
                req.Options.Set(WebAssemblyEnableStreamingRequestKey, true);
            }
            var stream = (arg as HasStream).NotNull().Stream;
            req.Content = new StreamContent(stream.Data);
            req.Headers.Add(RpcHttp.DataHeader, RpcHttp.Serialize(arg).ToB64());
            req.Headers.Add(RpcHttp.ContentNameHeader, stream.Name);
            req.Content.Headers.Add(RpcHttp.ContentTypeHeader, stream.Type);
            req.Content.Headers.Add(RpcHttp.ContentLengthHeader, stream.Size.ToString());
        }
        else if (typeof(TArg) != Nothing.Type)
        {
            req.Content = new ByteArrayContent(RpcHttp.Serialize(arg));
        }

        using var resp = await _client.NotNull().SendAsync(req, ctkn);
        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync(ctkn);
            _rpcExceptionHandler(msg);
            throw new RpcException(msg, (int)resp.StatusCode);
        }

        if (!RpcHttp.HasStream<TRes>())
        {
            if (arg is FcmUnregister)
            {
                _client.DefaultRequestHeaders.Remove(Fcm.ClientHeaderName);
            }
            if (typeof(TRes) == Nothing.Type)
            {
                return (Nothing.Inst as TRes).NotNull();
            }
            var bs = await resp.Content.ReadAsByteArrayAsync(ctkn);
            var tRes = RpcHttp.Deserialize<TRes>(bs).NotNull();
            if (tRes is FcmRegisterRes regRes)
            {
                // if we've registered to an fcm topic, we should always be making requests
                // with fcm client header
                _client.DefaultRequestHeaders.Remove(Fcm.ClientHeaderName);
                _client.DefaultRequestHeaders.Add(Fcm.ClientHeaderName, regRes.Client);
            }

            return tRes;
        }

        var resBs = resp.Headers.GetValues(RpcHttp.DataHeader).First().FromB64();
        var res = RpcHttp.Deserialize<TRes>(resBs).NotNull();
        var sub = (res as HasStream).NotNull();
        var cd = new ContentDisposition(
            string.Join(";", resp.Headers.GetValues(RpcHttp.ContentDispositionHeader))
        );
        sub.Stream = new RpcStream(
            await resp.Content.ReadAsStreamAsync(ctkn),
            cd.FileName ?? "unnamed_file",
            cd.DispositionType,
            true,
            (ulong)cd.Size
        );
        return res;
    }

    public string GetUrl<TArg, TRes>(Rpc<TArg, TRes> rpc, TArg arg)
        where TArg : class
        where TRes : class
    {
        Throw.OpIf(
            RpcHttp.HasStream<TArg>(),
            "can't generate get url for an rpc whose arg has a stream"
        );
        return $"{_baseHref}{rpc.Path}?{RpcHttp.QueryParam}={RpcHttp.Serialize(arg).ToB64()}";
    }
}
