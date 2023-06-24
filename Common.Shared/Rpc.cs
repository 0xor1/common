using System.Net.Mime;
using Common.Shared.Auth;
using Newtonsoft.Json;

namespace Common.Shared;

public record Rpc<TArg, TRes>
    where TArg : class
    where TRes : class
{
    public string Path { get; }

    public Rpc(string path)
    {
        Path = path.ToLower();
    }
}

public record RpcStream(Stream Data, string Name, string Type, bool IsDownload, ulong Size)
    : IDisposable,
        IAsyncDisposable
{
    public void Dispose()
    {
        Data.Dispose();
    }

    public ValueTask DisposeAsync() => Data.DisposeAsync();
}

public record HasStream : IDisposable, IAsyncDisposable
{
    [JsonIgnore]
    public RpcStream Stream { get; set; }

    public void Dispose() => Stream.Dispose();

    public ValueTask DisposeAsync() => Stream.DisposeAsync();
}

public record Nothing
{
    public static readonly Type Type = typeof(Nothing);
    public static readonly Nothing Inst = new();

    private Nothing() { }
}

public class RpcException : Exception
{
    public RpcException(string message, int code = 500)
        : base(message)
    {
        Code = code;
    }

    public int Code { get; }
}

public interface IRpcClient
{
    Task<TRes> Do<TArg, TRes>(Rpc<TArg, TRes> rpc, TArg arg)
        where TArg : class
        where TRes : class;
    string GetUrl<TArg, TRes>(Rpc<TArg, TRes> rpc, TArg arg)
        where TArg : class
        where TRes : class;
}

public record RpcHttpClient : IRpcClient
{
    private readonly string _baseHref;
    private readonly HttpClient _client;
    private readonly Action<string> _rpcExceptionHandler;

    public RpcHttpClient(string baseHref, HttpClient client, Action<string> reh)
    {
        _baseHref = baseHref + "api";
        _client = client;
        _rpcExceptionHandler = reh;
    }

    public async Task<TRes> Do<TArg, TRes>(Rpc<TArg, TRes> rpc, TArg arg)
        where TArg : class
        where TRes : class
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, _baseHref + rpc.Path);
        if (RpcHttp.HasStream<TArg>())
        {
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

        using var resp = await _client.NotNull().SendAsync(req);
        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
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
            var bs = await resp.Content.ReadAsByteArrayAsync();
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
            await resp.Content.ReadAsStreamAsync(),
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

public static class RpcHttp
{
    public const string QueryParam = "arg";
    public const string DataHeader = "X-Data";
    public const string ContentNameHeader = "X-Content-Name";
    public const string ContentTypeHeader = "Content-Type";
    public const string ContentLengthHeader = "Content-Length";
    public const string ContentDispositionHeader = "Content-Disposition";

    public static byte[] Serialize(object v) => Json.From(v).ToUtf8Bytes();

    public static T Deserialize<T>(byte[] bs)
        where T : class => Json.To<T>(bs.FromUtf8Bytes());

    public static bool HasStream<T>() => typeof(T).IsAssignableTo(typeof(HasStream));
}
