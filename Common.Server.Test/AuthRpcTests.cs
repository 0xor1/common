using Common.Server.Auth;
using Common.Shared.Auth;

namespace Common.Server.Test;

public class AuthRpcTests : IDisposable
{
    private readonly RpcTestRig<CommonTestDb, Api> _rpcTestRig;

    public AuthRpcTests()
    {
        _rpcTestRig = new RpcTestRig<CommonTestDb, Api>(
            S.Inst,
            new AuthEps<CommonTestDb>(
                0,
                (_, _, _) => Task.CompletedTask,
                (_, _, _, _) => Task.CompletedTask
            ).Eps,
            client => new Api(client),
            (sp) => Task.CompletedTask
        );
    }

    [Fact]
    public async Task FullRegistrationAndSignInFlow_Success()
    {
        var (ali, _, _) = await _rpcTestRig.NewApi("ali");
        var ses = await ali.Auth.GetSession();
        Assert.True(ses.IsAuthed);
    }

    [Fact]
    public async Task Register_ThrowsOnInvalidEmail()
    {
        var (api, _, _) = await _rpcTestRig.NewApi();
        try
        {
            await api.Auth.Register(new("invalid_email", "asdASD123@"));
        }
        catch (RpcTestException ex)
        {
            Assert.Equal(
                S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthInvalidEmail),
                ex.Rpc.Message
            );
        }
    }

    [Fact]
    public async Task Register_ThrowsOnInvalidPwd()
    {
        var (api, _, _) = await _rpcTestRig.NewApi();
        try
        {
            await api.Auth.Register(new("ali@ali.ali", "abc123@="));
        }
        catch (RpcTestException ex)
        {
            Assert.Equal(
                $"{S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthInvalidPwd)}:\n{S.Inst.GetOrAddress(S.DefaultLang, Shared.S.AuthNoUpperCaseChar)}",
                ex.Rpc.Message
            );
        }
    }

    [Fact]
    public async Task FullResetPwdFlow_Success()
    {
        var (ali, email, _) = await _rpcTestRig.NewApi("ali");
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
        var (ali, _, _) = await _rpcTestRig.NewApi("ali");
        var ses = await ali.Auth.SetL10n(new("es", "MM-dd-yyyy", "h:mmtt"));
        Assert.Equal("es", ses.Lang);
        Assert.Equal("MM-dd-yyyy", ses.DateFmt);
        Assert.Equal("h:mmtt", ses.TimeFmt);
    }

    [Fact]
    public async Task FcmEnabled_Success()
    {
        var (ali, _, _) = await _rpcTestRig.NewApi("ali");
        var ses = await ali.Auth.FcmEnabled(new(true));
        Assert.True(ses.FcmEnabled);
        // same no change
        ses = await ali.Auth.FcmEnabled(new(true));
        Assert.True(ses.FcmEnabled);
        ses = await ali.Auth.FcmEnabled(new(false));
        Assert.False(ses.FcmEnabled);
    }

    [Fact]
    public async Task FcmRegister_Success()
    {
        var (ali, _, _) = await _rpcTestRig.NewApi("ali");
        var ses = await ali.Auth.FcmEnabled(new(true));
        var res = await ali.Auth.FcmRegister(new(new List<string>() { "a", "b" }, "a", null));
        Assert.NotEmpty(res.Client);
    }

    [Fact]
    public async Task FcmRegister_6_Times_Success()
    {
        var (ali, _, _) = await _rpcTestRig.NewApi("ali");
        var ses = await ali.Auth.FcmEnabled(new(true));
        var res = await ali.Auth.FcmRegister(new(new List<string>() { "a", "b" }, "a", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a", "b", "c" }, "b", res.Client));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "c", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "d", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "e", null));
        res = await ali.Auth.FcmRegister(new(new List<string>() { "a", "b" }, "a", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a", "b", "c" }, "b", res.Client));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "c", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "d", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "e", null));
        Assert.NotEmpty(res.Client);
    }

    [Fact]
    public async Task FcmUnregister_Success()
    {
        var (ali, _, _) = await _rpcTestRig.NewApi("ali");
        var ses = await ali.Auth.FcmEnabled(new(true));
        var res = await ali.Auth.FcmRegister(new(new List<string>() { "a", "b" }, "a", null));
        await ali.Auth.FcmUnregister();
    }

    public void Dispose()
    {
        _rpcTestRig.Dispose();
    }
}
