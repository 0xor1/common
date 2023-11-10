namespace Common.Shared;

public interface IRpcClient
{
    Task<TRes> Do<TArg, TRes>(Rpc<TArg, TRes> rpc, TArg arg, CancellationToken ctkn = default)
        where TArg : class
        where TRes : class;
    string GetUrl<TArg, TRes>(Rpc<TArg, TRes> rpc, TArg arg)
        where TArg : class
        where TRes : class;
}
