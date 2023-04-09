using MessagePack;

namespace Common.Shared.Api;

// shared
public record Rpc(string Path)
{
    public const string DataHeader = "X-Data";

    public static byte[] Serialize<T>(T v) => MessagePackSerializer.Serialize(v);
    public static T Deserialize<T>(byte[] bs) => MessagePackSerializer.Deserialize<T>(bs);
}

public record DataStream
{
    [IgnoreMember]
    public Stream Data { get; set; }
}

[MessagePackObject]
public record Nothing;

[MessagePackObject]
public record MyArg
{
    [Key(0)]
    public string Val { get; init; }
}

[MessagePackObject]
public record MyRes
{
    [Key(0)] 
    public string ResVal { get; init; }
}

// client side
public record RpcClient<TArg, TRes>(string Path) : Rpc(Path) where TArg : class, new() where TRes : class, new()
{
    public async Task<TRes> Call(HttpClient client, TArg arg)
    {
        var argsBs = Serialize(arg);
        using var req = new HttpRequestMessage(HttpMethod.Post, Path);
        if (arg is DataStream sArg)
        {
            req.Content = new StreamContent(sArg.Data);
            req.Headers.Add(DataHeader, argsBs.ToB64());
        }
        else
        {
            req.Content = new ByteArrayContent(argsBs);
        }

        using var resp = await client.SendAsync(req);
        
        if (typeof(TRes).IsSubclassOf(typeof(DataStream)))
        {
            var resBs = resp.Headers.GetValues(DataHeader).First().FromB64();
            var res = Deserialize<TRes>(resBs).NotNull();
            var sub = (res as DataStream).NotNull();
            sub.Data = await resp.Content.ReadAsStreamAsync();
            return res;
        }
        return Deserialize<TRes>(await resp.Content.ReadAsByteArrayAsync()).NotNull();
    }
}

// server side
public record RpcServer<TArg, TRes>(string Key) : Rpc(Key) where TArg : new() where TRes : new()
{
    public async Task<TRes> Call(HttpClient client, TArg arg)
    {
        var sendArgs = MessagePackSerializer.Serialize(arg);
        var req = new HttpRequestMessage(HttpMethod.Post, Path);
        if (arg is DataStream sArg)
        {
            req.Content = new StreamContent(sArg.Data);
            req.Headers.Add(Rpc.DataHeader, sendArgs.ToB64());
        }
        else
        {
            req.Content = new ByteArrayContent(sendArgs);
        }
    }
}
