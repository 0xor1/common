namespace Common.Server;

public interface IEp
{
    string Path { get; }

    ulong? MaxSize { get; }
    Task Execute(IRpcCtxInternal ctx);
}
