using Common.Shared.I18n;
using Radzen;

namespace Common.Client;

public class ErrorInterceptor
{
    private readonly NotificationService NotificationService;
    private readonly L L;
    
    public ErrorInterceptor(NotificationService notificationService, L l)
    {
        NotificationService = notificationService;
        L = l;
    }

    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> t)
    {
        try
        {
            return await t;
        }
        catch (Exception ex)
        {
            var code = StatusCode.Internal;
            var level = NotificationSeverity.Error;
            var message = S.UnexpectedError;
            if (ex.GetType() == typeof(RpcException))
            {
                var rpc = (RpcException)ex;
                code = rpc.Status.StatusCode;
                message = rpc.Status.Detail;
                Console.WriteLine($"{DateTime.UtcNow.ToString("s")} {code} - {L.S(message)}");
            }
            else
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString("s")} {ex.Message}");
            }

            NotificationService.Notify(level, "Api Error", L.S(message), duration: 10000D);
            // rethrow in case any other specific components need to handle it too.
            throw new ApiException(code, message);
        }
    }
}
