using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;

namespace Common.Server.Test;

public class AuthRpcTests : IDisposable
{
    private readonly RpcTestRig<CommonTestDb> _rpcTestRig;

    public AuthRpcTests()
    {
        _rpcTestRig = new RpcTestRig<CommonTestDb>(S.Inst, AuthEps<CommonTestDb>.Eps);
    }

    [Fact]
    public async Task FullRegistrationAndSignInFlow_Success()
    {
        var (ali, _, _) = await NewApi("ali");
        var ses = await ali.Auth.GetSession();
        Assert.True(ses.IsAuthed);
    }

    [Fact]
    public async Task Register_ThrowsOnInvalidEmail()
    {
        var (api, _, _) = await NewApi();
        try
        {
            await api.Auth.Register(new("invalid_email", "asdASD123@"));
        }
        catch (RpcException ex)
        {
            Assert.Equal(S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthInvalidEmail), ex.Message);
        }
    }

    [Fact]
    public async Task Register_ThrowsOnInvalidPwd()
    {
        var (api, _, _) = await NewApi();
        try
        {
            await api.Auth.Register(new("ali@ali.ali", "abc123@="));
        }
        catch (RpcException ex)
        {
            Assert.Equal(
                $"{S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthInvalidPwd)}:\n{S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthNoUpperCaseChar)}",
                ex.Message
            );
        }
    }

    [Fact]
    public async Task FullResetPwdFlow_Success()
    {
        var (ali, email, _) = await NewApi("ali");
        await ali.Auth.SignOut();
        await ali.Auth.SendResetPwdEmail(new(email));
        var pwdCode = _rpcTestRig.RunDb(
            (db) => db.Auths.Single(x => x.Email == email).ResetPwdCode
        );
        var newPwd = "asdASD123@=";
        await ali.Auth.ResetPwd(new(email, pwdCode, newPwd));
        var ses = await ali.Auth.SignIn(new(email, newPwd, false));
        Assert.True(ses.IsAuthed);
    }

    [Fact]
    public async Task SetL10n_Success()
    {
        var (ali, _, _) = await NewApi("ali");
        var ses = await ali.Auth.SetL10n(new("es", "MM-dd-yyyy", "h:mmtt"));
        Assert.Equal("es", ses.Lang);
        Assert.Equal("MM-dd-yyyy", ses.DateFmt);
        Assert.Equal("h:mmtt", ses.TimeFmt);
    }

    public async Task<(IApi, string Email, string Pwd)> NewApi(string? name = null) =>
        await _rpcTestRig.NewApi(rpcClient => new Api(rpcClient), name);

    public void Dispose()
    {
        _rpcTestRig.Dispose();
    }
}
