namespace Common.Server;

public interface IRpcCtxInternal : IRpcCtx
{
    Task<T> GetArg<T>()
        where T : class;
    Task WriteResp<T>(T val)
        where T : class;
    Task HandleException(Exception ex, string message, int code);
}
