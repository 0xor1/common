using Common.Shared;

namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<string, TemplatableString> English = new Dictionary<
        string,
        TemplatableString
    >()
    {
        { Demo, new("Demo") },
        {
            DemoTitle,
            new(
                "This app is for demonstration purposes only.\nAll Data may be erased at anytime with no warning."
            )
        },
        { Invalid, new("Invalid") },
        { RpcUnknownEndpoint, new("Unknown RPC endpoint") },
        { UnexpectedError, new("An unexpected error occurred") },
        { EntityNotFound, new("{{Name}} not found") },
        { NotFound, new("Not found") },
        { NothingAtAddress, new("Sorry, nothing at this address.") },
        { InsufficientPermission, new("Insufficient permission") },
        { ApiError, new("Api Error") },
        { MinMaxNullArgs, new("Both min and  max arguments are null") },
        { MinMaxReversedArgs, new("Min {{Min}} and Max {{Max}} values are out of ordered") },
        { BadRequest, new("Bad request") },
        { RequestBodyTooLarge, new("Request body too large, limit {{MaxSize}} bytes") },
        { AuthInvalidEmail, new("Invalid email") },
        { AuthInvalidPwd, new("Invalid password") },
        { AuthLessThan8Chars, new("Less than 8 characters") },
        { AuthNoLowerCaseChar, new("No lowercase character") },
        { AuthNoUpperCaseChar, new("No uppercase character") },
        { AuthNoDigit, new("No digit") },
        { AuthNoSpecialChar, new("No special character") },
        {
            AuthThousandsAndDecimalSeparatorsMatch,
            new("Thousands and decimal separator characters match")
        },
        { AuthAlreadyAuthenticated, new("Already in authenticated session") },
        { AuthNotAuthenticated, new("Not in authenticated session") },
        { AuthInvalidEmailCode, new("Invalid email code") },
        { AuthInvalidResetPwdCode, new("Invalid reset password code") },
        {
            AuthAccountNotVerified,
            new("Account not verified, please check your emails for verification link")
        },
        {
            AuthAttemptRateLimit,
            new(
                "Authentication attempts cannot be made more frequently than every {{Seconds}} seconds"
            )
        },
        { AuthConfirmEmailSubject, new("Confirm Email Address") },
        {
            AuthConfirmEmailHtml,
            new(
                "<div><a href=\"{{BaseHref}}/verify_email?email={{Email}}&code={{Code}}\">Please click this link to verify your email address</a></div >"
            )
        },
        {
            AuthConfirmEmailText,
            new(
                "Please use this link to verify your email address: {{BaseHref}}/verify_email?email={{Email}}&code={{Code}}"
            )
        },
        { AuthResetPwdSubject, new("Reset Password") },
        {
            AuthResetPwdHtml,
            new(
                "<div><a href=\"{{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}\">Please click this link to reset your password</a></div>"
            )
        },
        {
            AuthResetPwdText,
            new(
                "Please click this link to reset your password: {{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}"
            )
        },
        { AuthMagicLinkSubject, new("Magic Login Link") },
        {
            AuthMagicLinkHtml,
            new(
                "<div><a href=\"{{BaseHref}}/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}\">Please click this link to login</a></div>"
            )
        },
        {
            AuthMagicLinkText,
            new(
                "Please click this link to login: {{BaseHref}}/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}"
            )
        },
        { AuthFcmTopicInvalid, new("Fcm topic invalid Min: {{Min}}, Max: {{Max}}") },
        { AuthFcmTokenInvalid, new("Fcm token invalid") },
        { AuthFcmNotEnabled, new("Fcm not enabled") },
        { AuthFcmMessageInvalid, new("Fcm message invalid") },
        { L10n, new("Localization") },
        { ToggleLiveUpdates, new("Toggle live updates") },
        { Live, new("Live:") },
        { Or, new("Or") },
        { On, new("On") },
        { Off, new("Off") },
        { Language, new("Language") },
        { DateFmt, new("Date Format") },
        { TimeFmt, new("Time Format") },
        { ThousandsSeparator, new("Thousands Separator") },
        { DecimalSeparator, new("Decimal Separator") },
        { Register, new("Register") },
        { Registering, new("Registering") },
        {
            RegisterSuccess,
            new("Please check your emails for a confirmation link to complete registration.")
        },
        { SignIn, new("Sign In") },
        { RememberMe, new("Remember Me") },
        { SigningIn, new("Signing In") },
        { SignOut, new("Sign Out") },
        { SigningOut, new("Signing Out") },
        { VerifyEmail, new("Verify Email") },
        { Verifying, new("Verifying") },
        { VerifyingEmail, new("Verifying your email") },
        { VerifyEmailSuccess, new("Thank you for verifying your email.") },
        { ResetPwd, new("Reset Password") },
        { MagicLink, new("Magic Link") },
        { MagicLinkSignIn, new("Magic Link Sign In") },
        { Email, new("Email") },
        { Pwd, new("Password") },
        { ConfirmPwd, new("Confirm Password") },
        { PwdsDontMatch, new("Passwords don't match") },
        { ResetPwdSuccess, new("You can now sign in with your new password.") },
        { Resetting, new("Resetting") },
        { SendResetPwdLink, new("Send Reset Password Link") },
        {
            SendResetPwdLinkSuccess,
            new(
                "If this email matches an account an email will have been sent to reset your password."
            )
        },
        { SendMagicLink, new("Send Magic Link") },
        {
            SendMagicLinkSuccess,
            new(
                "If this email matches an account an email will have been sent with your magic login link."
            )
        },
        { Processing, new("Processing") },
        { Send, new("Send") },
        { Success, new("Success") },
        { Error, new("Error") },
        { Update, new("Update") },
        { Updating, new("Updating") },
    };
}
