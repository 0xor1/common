using System.Reflection;
using System.Text;
using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;

namespace Common.Server.Test;

public class AuthRpcTests
{
    private readonly RpcTestRig<CommonTestDb> _rpcTestRig;
    private readonly IAuthApi _ali;

    public AuthRpcTests()
    {
        var ass = Assembly.GetExecutingAssembly();
        var configName = ass.GetManifestResourceNames().Single(x => x.EndsWith("config.json"));
        var configStream = ass.GetManifestResourceStream(configName).NotNull();
        var streamReader = new StreamReader(configStream);
        var configStr = streamReader.ReadToEnd();
        _rpcTestRig = new RpcTestRig<CommonTestDb>(Config.FromJson(configStr), S.Inst, AuthEps<CommonTestDb>.Eps);
        _ali = new AuthApi(_rpcTestRig.NewClient());
    }

    [Fact]
    public async Task Test1()
    {
        await _ali.Register(new ("ali@ali.ali", "asdASD123@"));
    }
}