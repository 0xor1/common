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
        where T : class
    {
        return JsonConvert.DeserializeObject<T>(bs.FromUtf8Bytes()).NotNull();
    }

    public static bool HasStream<T>()
    {
        return typeof(IStream).IsAssignableFrom(typeof(T));
    }
}

public record Rpc<TArg, TRes>(string Path)
    where TArg : class
    where TRes : class
{
    private static HttpClient? _client;

    public void Init(HttpClient client)
    {
        _client ??= client;
    }

    public async Task<TRes> Do(TArg arg)
    {
        var argsBs = Rpc.Serialize(arg);
        using var req = new HttpRequestMessage(HttpMethod.Post, Path);
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

        if (!Rpc.HasStream<TRes>())
            return Rpc.Deserialize<TRes>(await resp.Content.ReadAsByteArrayAsync()).NotNull();

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
