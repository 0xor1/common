namespace Common.Shared.Auth;

public class Api : IApi
{
    public Api(IRpcClient client)
    {
        App = new AppApi(client);
        Auth = new AuthApi(client);
    }

    public IAppApi App { get; }
    public IAuthApi Auth { get; }
}
