using MessagePack;
using Newtonsoft.Json;

namespace Common.Shared;

public record HasStream : IDisposable, IAsyncDisposable
{
    [JsonIgnore]
    [IgnoreMember]
    public RpcStream Stream { get; set; } = null!;

    public void Dispose() => Stream.Dispose();

    public ValueTask DisposeAsync() => Stream.DisposeAsync();
}
