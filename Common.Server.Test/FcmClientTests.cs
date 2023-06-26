using Common.Server.Auth;
using Common.Shared.Auth;
using FirebaseAdmin.Messaging;

namespace Common.Server.Test;

public class FcmClientTests : IDisposable
{
    private readonly RpcTestRig<CommonTestDb, Api> _rpcTestRig;

    public FcmClientTests()
    {
        _rpcTestRig = new RpcTestRig<CommonTestDb, Api>(
            S.Inst,
            new AuthEps<CommonTestDb>(
                0,
                (_, _, _) => Task.CompletedTask,
                (_, _, _, _) => Task.CompletedTask
            ).Eps,
            client => new Api(client),
            (sc) => { },
            (sp) => Task.CompletedTask
        );
    }

    [Fact]
    public async Task Send_Success()
    {
        var c = _rpcTestRig.Get<IFcmClient>();
        var res = await c.Send(
            new MulticastMessage()
            {
                Tokens = new List<string>() { },
                Data = new Dictionary<string, string>() { { "yolo", "solo" } }
            }
        );
        Assert.Equal(0, res.SuccessCount + res.FailureCount);
        res = await c.Send(
            new MulticastMessage()
            {
                Tokens = new List<string>() { "a", "b" },
                Data = new Dictionary<string, string>() { { "yolo", "solo" } }
            }
        );
        // todo figure out how to get actual reg token to send data
        Assert.Equal(2, res.SuccessCount + res.FailureCount);
    }

    public void Dispose()
    {
        _rpcTestRig.Dispose();
    }
}
