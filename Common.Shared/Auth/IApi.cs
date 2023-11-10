namespace Common.Shared.Auth;

public interface IApi
{
    public IAppApi App { get; }
    public IAuthApi Auth { get; }
}
