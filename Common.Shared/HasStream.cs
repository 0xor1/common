using MessagePack;
using Newtonsoft.Json;

namespace Common.Shared;

public record HasStream(RpcStream Stream) : IDisposable, IAsyncDisposable
{
    [JsonIgnore]
    [IgnoreMember]
    public RpcStream Stream { get; set; } = Stream;

    public void Dispose() => Stream.Dispose();

    public ValueTask DisposeAsync() => Stream.DisposeAsync();
}
