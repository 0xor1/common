namespace Common.Shared;

public class RpcException : Exception
{
    public RpcException(string message, int code = 500)
        : base(message)
    {
        Code = code;
    }

    public int Code { get; }
}
