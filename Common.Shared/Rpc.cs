namespace Common.Shared;

public record Rpc<TArg, TRes>
    where TArg : class
    where TRes : class
{
    public string Path { get; }

    public ulong? MaxSize { get; }

    public Rpc(string path, ulong? maxSize = Size.KB)
    {
        Path = path.ToLower();
        MaxSize = maxSize;
    }
}
