namespace Common.Shared.Auth;

public static class AuthRpcs
{
    public static readonly Rpc<Nothing, Session> GetSession = new("/auth/get_session");
    public static readonly Rpc<Register, Nothing> Register = new("/auth/register");
    public static readonly Rpc<VerifyEmail, Nothing> VerifyEmail = new("/auth/verify_email");
    public static readonly Rpc<SendResetPwdEmail, Nothing> SendResetPwdEmail =
        new("/auth/send_reset_pwd_email");
    public static readonly Rpc<ResetPwd, Nothing> ResetPwd = new("/auth/reset_pwd");
    public static readonly Rpc<SendMagicLinkEmail, Nothing> SendMagicLinkEmail =
        new("/auth/send_magic_link_email");
    public static readonly Rpc<MagicLinkSignIn, Session> MagicLinkSignIn =
        new("/auth/magic_link_sign_in");
    public static readonly Rpc<SignIn, Session> SignIn = new("/auth/sign_in");
    public static readonly Rpc<Nothing, Session> SignOut = new("/auth/sign_out");
    public static readonly Rpc<Nothing, Session> Delete = new("/auth/delete");
    public static readonly Rpc<SetL10n, Session> SetL10n = new("/auth/set_l10n");
    public static readonly Rpc<FcmEnabled, Session> FcmEnabled = new("/auth/fcm_enabled");
    public static readonly Rpc<FcmRegister, FcmRegisterRes> FcmRegister = new("/auth/fcm_register");
    public static readonly Rpc<FcmUnregister, Nothing> FcmUnregister = new("/auth/fcm_unregister");
}
