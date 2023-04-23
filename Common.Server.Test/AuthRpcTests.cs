using System.Reflection;
using System.Text;
using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;

namespace Common.Server.Test;

public class AuthRpcTests : IAsyncDisposable
{
    private readonly RpcTestRig<CommonTestDb> _rpcTestRig;

    public AuthRpcTests()
    {
        _rpcTestRig = new RpcTestRig<CommonTestDb>(S.Inst, AuthEps<CommonTestDb>.Eps);
    }

    [Fact]
    public async Task BasicRegistrationFlowSuccess()
    {
        var ali = await NewApi("ali");
        var ses = await ali.GetSession();
        Assert.True(ses.IsAuthed);
    }

    [Fact]
    public async Task Register_ThrowsOnInvalidEmail()
    {
        var api = await NewApi();
        try
        {
            await api.Register(new("invalid_email", "asdASD123@"));
        }
        catch (RpcException ex)
        {
            Assert.Equal(S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthInvalidEmail), ex.Message);
        }
    }

    public async Task<IAuthApi> NewApi(string? name = null) =>
        await _rpcTestRig.NewApi(rpcClient => new AuthApi(rpcClient), name);

    public async ValueTask DisposeAsync()
    {
        await _rpcTestRig.DisposeAsync();
    }
}
