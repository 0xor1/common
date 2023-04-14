using System.Reflection.Metadata;
using Newtonsoft.Json;

namespace Common.Shared;

public static class Rpc
{
    public const string QueryParam = "arg";
    public const string DataHeader = "X-Data";

    public static byte[] Serialize(object? v)
    {
        return JsonConvert.SerializeObject(v).ToUtf8Bytes();
    }

    public static T Deserialize<T>(byte[] bs)
        where T : class => JsonConvert.DeserializeObject<T>(bs.FromUtf8Bytes()).NotNull();

    public static bool HasStream<T>() => typeof(IStream).IsAssignableFrom(typeof(T));
}

public record RpcBase
{
    protected static string? _baseHref;
    protected static HttpClient? _client;
    protected static Action<string>? _rpcExceptionHandler;

    public static void Init(string baseHref, HttpClient client, Action<string> reh)
    {
        _baseHref ??= baseHref + "api";
        _client ??= client;
        _rpcExceptionHandler ??= reh;
    }
}

public record Rpc<TArg, TRes> : RpcBase
    where TArg : class
    where TRes : class
{
    public Rpc(string path)
    {
        Path = path.ToLower();
    }

    public string Path { get; }

    public async Task<TRes> Do(TArg arg)
    {
        var argsBs = Rpc.Serialize(arg);
        using var req = new HttpRequestMessage(HttpMethod.Post, _baseHref + Path);
        if (Rpc.HasStream<TArg>())
        {
            req.Content = new StreamContent((arg as IStream).NotNull().Stream);
            req.Headers.Add(Rpc.DataHeader, argsBs.ToB64());
        }
        else
        {
            req.Content = new ByteArrayContent(argsBs);
        }

        using var resp = await _client.NotNull().SendAsync(req);
        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            _rpcExceptionHandler(msg);
        }

        if (!Rpc.HasStream<TRes>())
        {
            var bs = await resp.Content.ReadAsByteArrayAsync();
            return Rpc.Deserialize<TRes>(bs).NotNull();
        }

        var resBs = resp.Headers.GetValues(Rpc.DataHeader).First().FromB64();
        var res = Rpc.Deserialize<TRes>(resBs).NotNull();
        var sub = (res as IStream).NotNull();
        sub.Stream = await resp.Content.ReadAsStreamAsync();
        return res;
    }
}

public interface IStream
{
    public Stream Stream { get; set; }
}

public record Nothing;
