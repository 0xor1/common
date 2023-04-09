using Common.Shared;

namespace Common.Client.Auth;

public interface IAuthApi
{
    Task<Nothing> Register(RegisterReq req);
    private static IAuthApi? _inst;
    public static IAuthApi Init(HttpClient client) => _inst ??= new AuthApi(client);
}

internal record AuthApi: IAuthApi
{
    private readonly HttpClient _client;

    internal AuthApi(HttpClient client)
    {
        _client = client;
    }

    public async Task<Nothing> Register(RegisterReq req)
        => await Rpc.Do<RegisterReq, Nothing>(_client, req);
    
}

public record RegisterReq :IRpcReq
{
    public static string Path => "/auth/register";
}
public static class Test{
    public static async Task Run()
    {
        var api = IAuthApi.Init(new HttpClient());
        await api.Register(new ()
        {
            
        });
    }
}