namespace Common.Shared.Auth;

public static class AppRpcs
{
    public static readonly Rpc<Nothing, Config> GetConfig = new("/app/get_config");
}
