using System.Reflection;
using System.Text;
using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;

namespace Common.Server.Test;

public class AuthRpcTests: IAsyncDisposable
{
    private readonly RpcTestRig<CommonTestDb> _rpcTestRig;

    public AuthRpcTests()
    {
        _rpcTestRig = new RpcTestRig<CommonTestDb>(S.Inst, AuthEps<CommonTestDb>.Eps);
    }

    [Fact]
    public async Task AuthApi_BasicRegistrationFlowSuccess()
    {
        var ali = await _rpcTestRig.NewApi((rpcClient) => new AuthApi(rpcClient), "ali");
        var ses = await ali.GetSession();
        Assert.True(ses.IsAuthed);
    }

    public async ValueTask DisposeAsync()
    {
        await _rpcTestRig.DisposeAsync();
    }
}