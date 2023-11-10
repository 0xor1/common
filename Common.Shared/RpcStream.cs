namespace Common.Shared;

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
