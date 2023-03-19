using Grpc.Core;

namespace Common.Client;

public class ApiException : Exception
{
    public StatusCode Code { get; }

    public ApiException(StatusCode code, string message)
        : base(message)
    {
        Code = code;
    }
}
