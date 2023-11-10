namespace Common.Server;

public interface IRpcEndpoint
{
    string Path { get; }

    long? MaxSize { get; }
    Task Execute(IRpcCtxInternal ctx);
}
