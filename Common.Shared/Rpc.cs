namespace Common.Shared;

public record Rpc<TArg, TRes>
    where TArg : class
    where TRes : class
{
    public string Path { get; }

    public long? MaxSize { get; }

    public Rpc(string path, long? maxSize = Size.KB)
    {
        Path = path.ToLower();
        MaxSize = maxSize;
    }
}
