namespace Common.Server;

public interface IRpcEndpoint
{
    string Path { get; }

    ulong? MaxSize { get; }
    Task Execute(IRpcCtxInternal ctx);
}
