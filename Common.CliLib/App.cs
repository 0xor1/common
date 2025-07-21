using IApi = Common.Shared.Auth.IApi;

namespace Common.CliLib;

public class App<TApi>
    where TApi : IApi
{
    private readonly TApi _api;

    public App(TApi api)
    {
        _api = api;
    }

    /// <summary>
    /// Get the app configuration
    /// </summary>
    public async Task GetConfig() => Io.WriteYml(await _api.App.GetConfig());
}
