namespace Common.Shared.Auth;

public class AppApi : IAppApi
{
    private readonly IRpcClient _client;

    public AppApi(IRpcClient client)
    {
        _client = client;
    }

    public Task<Config> GetConfig(CancellationToken ctkn = default) =>
        _client.Do(AppRpcs.GetConfig, Nothing.Inst, ctkn);
}
