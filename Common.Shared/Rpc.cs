namespace Common.Shared;

/// <summary>
/// Defines a remote procedure call, the argument type, the result type, the unique path and the max request size.
/// </summary>
/// <typeparam name="TArg">The argument type.</typeparam>
/// <typeparam name="TRes">The result type.</typeparam>
public record Rpc<TArg, TRes>
    where TArg : class
    where TRes : class
{
    public string Path { get; }

    public ulong? MaxSize { get; }

    /// <summary>
    /// Create a new Rpc.
    /// </summary>
    /// <param name="path">The path must be unique for your system.</param>
    /// <param name="maxSize">The maximum request size the server will accept.</param>
    public Rpc(string path, ulong? maxSize = Size.MB)
    {
        Path = path.ToLower();
        MaxSize = maxSize;
    }
}
