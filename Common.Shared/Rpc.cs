using System.Net;
using MessagePack;

namespace Common.Shared;

public interface IRpcReq
{
    public static abstract string Path { get; }
}

public static class Rpc
{
    public const string QueryParam = "arg";
    public const string DataHeader = "X-Data";

    public static byte[] Serialize<T>(T v) => MessagePackSerializer.Serialize(v);

    public static T Deserialize<T>(byte[] bs) => MessagePackSerializer.Deserialize<T>(bs);

    public static bool HasStream<T>() => typeof(T).IsSubclassOf(typeof(DataStream));

    public static async Task<TRes> Do<TArg, TRes>(HttpClient client, TArg arg)
        where TArg : class, IRpcReq, new()
        where TRes : class, new()
    {
        var argsBs = Serialize(arg);
        using var req = new HttpRequestMessage(HttpMethod.Post, TArg.Path);
        if (HasStream<TArg>())
        {
            req.Content = new StreamContent((arg as DataStream).NotNull().Data);
            req.Headers.Add(Rpc.DataHeader, argsBs.ToB64());
        }
        else
        {
            req.Content = new ByteArrayContent(argsBs);
        }

        using var resp = await client.SendAsync(req);

        if (!HasStream<TRes>())
        {
            return Deserialize<TRes>(await resp.Content.ReadAsByteArrayAsync()).NotNull();
        }

        var resBs = resp.Headers.GetValues(Rpc.DataHeader).First().FromB64();
        var res = Deserialize<TRes>(resBs).NotNull();
        var sub = (res as DataStream).NotNull();
        sub.Data = await resp.Content.ReadAsStreamAsync();
        return res;
    }
}

public record DataStream
{
    [IgnoreMember]
    public Stream Data { get; set; }
}

[MessagePackObject]
public record Nothing;

[MessagePackObject]
public record OnlyDataStream : DataStream;
