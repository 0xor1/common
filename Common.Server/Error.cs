using System.Net;
using Common.Shared;
using Common.Shared.I18n;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Server;

public class ApiException : Exception
{
    public HttpStatusCode Code { get; }

    public ApiException(string message, HttpStatusCode code = HttpStatusCode.InternalServerError)
        : base(message)
    {
        Code = code;
    }
}

public static class ErrorMiddleware
{
    public static IApplicationBuilder UseApiErrorHandling(this IApplicationBuilder app)
        => app.Use(async (ctx, next) =>
        {
            try
            {
                await next(ctx);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(ApiException))
                {
                    // TODO write error to response writer
                    throw;
                }
                ctx.RequestServices.GetRequiredService<ILogger>().LogError(ex, $"Error thrown by {ctx.Request.Path}");
                throw new ApiException(S.UnexpectedError);
            }
        });
}
