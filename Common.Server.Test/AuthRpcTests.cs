using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;

namespace Common.Server.Test;

public class AuthRpcTests : IDisposable
{
    private readonly RpcTestRig<CommonTestDb, Api> _rpcTestRig;

    public AuthRpcTests()
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
    public async Task FullRegistrationAndSignInFlow_Success()
    {
        var (ali, _, _) = await _rpcTestRig.NewApi("ali");
        var ses = await ali.Auth.GetSession();
        Assert.True(ses.IsAuthed);
    }

    [Fact]
    public async Task MagicLinkFlow_Success()
    {
        var (ali, email, _) = await _rpcTestRig.NewApi("ali");
        var ses = await ali.Auth.GetSession();
        await ali.Auth.SignOut();
        await ali.Auth.SendMagicLinkEmail(new(email, true));
        var code = _rpcTestRig.RunDb((db) => db.Auths.Single(x => x.Email == email).MagicLinkCode);
        ses = await ali.Auth.MagicLinkSignIn(new(email, code, true));
        Assert.True(ses.IsAuthed);
        Assert.True(ses.RememberMe);
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
                S.Inst.GetOrAddress(S.DefaultLang, Shared.I18n.S.AuthInvalidEmail),
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
            await api.Auth.Register(new("ali@ali.ali", "abc123abc"));
        }
        catch (RpcTestException ex)
        {
            Assert.Equal(
                $"{S.Inst.GetOrAddress(S.DefaultLang, Shared.I18n.S.AuthInvalidPwd)}:\n  {S.Inst.GetOrAddress(S.DefaultLang, Shared.I18n.S.AuthNoUpperCaseChar)}\n  {S.Inst.GetOrAddress(S.DefaultLang, Shared.I18n.S.AuthNoSpecialChar)}",
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
        var ses = await ali.Auth.SetL10n(new("es", DateFmt.MDY, "h:mmtt", "/", ".", ","));
        Assert.Equal("es", ses.Lang);
        Assert.Equal(DateFmt.MDY, ses.DateFmt);
        Assert.Equal("h:mmtt", ses.TimeFmt);
        Assert.Equal("/", ses.DateSeparator);
        Assert.Equal(".", ses.ThousandsSeparator);
        Assert.Equal(",", ses.DecimalSeparator);
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
        await ali.Auth.FcmRegister(new(new List<string>() { "a", "b", "c" }, "b", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "c", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "d", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "e", null));
        res = await ali.Auth.FcmRegister(new(new List<string>() { "a", "b" }, "a", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a", "b", "c" }, "b", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "c", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "d", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "e", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "f", null));
        await ali.Auth.FcmRegister(new(new List<string>() { "a" }, "g", res.Client));
        Assert.NotEmpty(res.Client);
    }

    [Fact]
    public async Task FcmUnregister_Success()
    {
        var (ali, _, _) = await _rpcTestRig.NewApi("ali");
        var ses = await ali.Auth.FcmEnabled(new(true));
        var res = await ali.Auth.FcmRegister(new(new List<string>() { "a", "b" }, "a", null));
        await ali.Auth.FcmUnregister(new(res.Client));
    }

    [Fact]
    public async Task AppGetConfig_Success()
    {
        var (ali, _, _) = await _rpcTestRig.NewApi("ali");
        var c = await ali.App.GetConfig();
        Assert.True(c.DemoMode);
        Assert.Equal("https://github.com/0xor1/common", c.RepoUrl);
    }

    public void Dispose()
    {
        _rpcTestRig.Dispose();
    }
}
