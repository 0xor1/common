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
            new CommonEps<CommonTestDb>(
                0,
                true,
                5,
                (_, _, _, _) => Task.CompletedTask,
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
        await c.Send(
            new Message()
            {
                Token = "a",
                Data = new Dictionary<string, string>() { { "yolo", "solo" } },
            }
        );
    }

    public void Dispose()
    {
        _rpcTestRig.Dispose();
    }
}
