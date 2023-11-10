using Common.Shared;

namespace Common.Server.Test;

public class RpcTestException : Exception
{
    public Exception Original { get; }
    public RpcException Rpc { get; }

    public RpcTestException(Exception original, RpcException rpc)
        : base(
            $"Original: {original.Message}\nRpc: {rpc.Message}\nOriginal Stacktrace: {original.StackTrace}\nRpc Stacktrace: {rpc.StackTrace}"
        )
    {
        Original = original;
        Rpc = rpc;
    }
};
