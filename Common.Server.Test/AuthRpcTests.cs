using System.Reflection;
using System.Text;
using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;
using Microsoft.EntityFrameworkCore;

namespace Common.Server.Test;

public class AuthRpcTests : IDisposable
{
    private readonly RpcTestRig<CommonTestDb> _rpcTestRig;

    public AuthRpcTests()
    {
        _rpcTestRig = new RpcTestRig<CommonTestDb>(S.Inst, AuthEps<CommonTestDb>.Eps);
    }

    [Fact]
    public async Task BasicRegistrationFlowSuccess()
    {
        var (ali, _, _) = await NewApi("ali");
        var ses = await ali.GetSession();
        Assert.True(ses.IsAuthed);
    }

    [Fact]
    public async Task Register_ThrowsOnInvalidEmail()
    {
        var (api, _, _) = await NewApi();
        try
        {
            await api.Register(new("invalid_email", "asdASD123@"));
        }
        catch (RpcException ex)
        {
            Assert.Equal(S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthInvalidEmail), ex.Message);
        }
    }

    [Fact]
    public async Task BasicResetPwdSuccess()
    {
        var (ali, email, _) = await NewApi("ali");
        await ali.SignOut();
        await ali.SendResetPwdEmail(new(email));
        await using var db = _rpcTestRig.GetDb();
        var pwdCode = (await db.Auths.FirstAsync(x => x.Email == email)).ResetPwdCode;
        var newPwd = "asdASD123@=";
        await ali.ResetPwd(new(email, pwdCode, newPwd));
        await Task.Delay(5000);
        var ses = await ali.SignIn(new(email, newPwd, false));
        Assert.True(ses.IsAuthed);
    }

    [Fact]
    public async Task Register_ThrowsOnInvalidPwd()
    {
        var (api, _, _) = await NewApi();
        try
        {
            await api.Register(new("ali@ali.ali", "abc123@="));
        }
        catch (RpcException ex)
        {
            Assert.Equal(
                $"{S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthInvalidPwd)}:\n{S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthNoUpperCaseChar)}",
                ex.Message
            );
        }
    }

    public async Task<(IAuthApi, string Email, string Pwd)> NewApi(string? name = null) =>
        await _rpcTestRig.NewApi(rpcClient => new AuthApi(rpcClient), name);

    public void Dispose()
    {
        _rpcTestRig.Dispose();
    }
}
