namespace Common.Shared.Auth;

public interface IAppApi
{
    Task<Config> GetConfig(CancellationToken ctkn = default);
}
