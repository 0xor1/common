
using System.Net;

namespace Common.Client;

public class ApiException : Exception
{
    public HttpStatusCode Code { get; }

    public ApiException(HttpStatusCode code, string message)
        : base(message)
    {
        Code = code;
    }
}
