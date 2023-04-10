using System.Net;

namespace Common.Client;

public class ApiException : Exception
{
    public ApiException(HttpStatusCode code, string message)
        : base(message)
    {
        Code = code;
    }

    public HttpStatusCode Code { get; }
}