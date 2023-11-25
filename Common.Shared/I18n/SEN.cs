// Generated Code File, Do Not Edit.
// This file is generated with Common.I18nCodeGen.

using Common.Shared;

namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<string, TemplatableString> EN_Strings =
        new Dictionary<string, TemplatableString>()
        {
            { ApiError, new("Api Error") },
            {
                AuthAccountNotVerified,
                new("Account not verified, please check your emails for verification link")
            },
            { AuthAlreadyAuthenticated, new("Already in authenticated session") },
            {
                AuthAttemptRateLimit,
                new(
                    "Authentication attempts cannot be made more frequently than every {{Seconds}} seconds"
                )
            },
            {
                AuthConfirmEmailHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/verify_email?email={{Email}}&code={{Code}}\">Please click this link to verify your email address</a></div >"
                )
            },
            { AuthConfirmEmailSubject, new("Confirm Email Address") },
            {
                AuthConfirmEmailText,
                new(
                    "Please use this link to verify your email address: {{BaseHref}}/cmn/auth/verify_email?email={{Email}}&code={{Code}}"
                )
            },
            { AuthFcmMessageInvalid, new("Fcm message invalid") },
            { AuthFcmNotEnabled, new("Fcm not enabled") },
            { AuthFcmTokenInvalid, new("Fcm token invalid") },
            { AuthFcmTopicInvalid, new("Fcm topic invalid Min: {{Min}}, Max: {{Max}}") },
            { AuthInvalidEmail, new("Invalid email") },
            { AuthInvalidEmailCode, new("Invalid email code") },
            { AuthInvalidPwd, new("Invalid password") },
            { AuthInvalidResetPwdCode, new("Invalid reset password code") },
            { AuthLessThan8Chars, new("Less than 8 characters") },
            {
                AuthMagicLinkHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}\">Please click this link to login</a></div>"
                )
            },
            { AuthMagicLinkSubject, new("Magic Login Link") },
            {
                AuthMagicLinkText,
                new(
                    "Please click this link to login: {{BaseHref}}/cmn/auth/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}"
                )
            },
            { AuthNoDigit, new("No digit") },
            { AuthNoLowerCaseChar, new("No lowercase character") },
            { AuthNoSpecialChar, new("No special character") },
            { AuthNoUpperCaseChar, new("No uppercase character") },
            { AuthNotAuthenticated, new("Not in authenticated session") },
            {
                AuthResetPwdHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/reset_pwd?email={{Email}}&code={{Code}}\">Please click this link to reset your password</a></div>"
                )
            },
            { AuthResetPwdSubject, new("Reset Password") },
            {
                AuthResetPwdText,
                new(
                    "Please click this link to reset your password: {{BaseHref}}/cmn/auth/reset_pwd?email={{Email}}&code={{Code}}"
                )
            },
            {
                AuthThousandsAndDecimalSeparatorsMatch,
                new("Thousands and decimal separator characters match")
            },
            { BadRequest, new("Bad request") },
            { ConfirmPwd, new("Confirm Password") },
            { DateFmt, new("Date Format") },
            { DecimalSeparator, new("Decimal Separator") },
            { DeleteAccount, new("Delete Account") },
            { DeleteAccountSuccess, new("Your account has been deleted.") },
            {
                DeleteAccountWarning,
                new(
                    "This will delete your account and all the data associated with it. Do you still want to delete your account?"
                )
            },
            { Deleting, new("Deleting") },
            { Demo, new("Demo") },
            {
                DemoTitle,
                new(
                    "This app is for demonstration purposes only.\nAll Data may be erased at anytime with no warning."
                )
            },
            { Email, new("Email") },
            { EntityNotFound, new("{{Name}} not found") },
            { Error, new("Error") },
            { FileTooLarge, new("File too large, limit {{MaxSize}}") },
            { InsufficientPermission, new("Insufficient permission") },
            { Invalid, new("Invalid") },
            { L10n, new("Localization") },
            { Language, new("Language") },
            { Live, new("Live:") },
            { LoadingSession, new("Loading Session") },
            { MagicLink, new("Magic Link") },
            { MagicLinkSignIn, new("Magic Link Sign In") },
            { MinMaxNullArgs, new("Both min and  max arguments are null") },
            { MinMaxReversedArgs, new("Min {{Min}} and Max {{Max}} values are out of ordered") },
            { No, new("No") },
            { NotFound, new("Not found") },
            { NothingAtAddress, new("Sorry, nothing at this address.") },
            { Off, new("Off") },
            { On, new("On") },
            { Or, new("Or") },
            { Processing, new("Processing") },
            { Pwd, new("Password") },
            { PwdsDontMatch, new("Passwords don't match") },
            { Register, new("Register") },
            {
                RegisterSuccess,
                new("Please check your emails for a confirmation link to complete registration.")
            },
            { Registering, new("Registering") },
            { RememberMe, new("Remember Me") },
            { RequestBodyTooLarge, new("Request body too large, limit {{MaxSize}} bytes") },
            { ResetPwd, new("Reset Password") },
            { ResetPwdSuccess, new("You can now sign in with your new password.") },
            { Resetting, new("Resetting") },
            { RpcUnknownEndpoint, new("Unknown RPC endpoint") },
            { Send, new("Send") },
            { SendMagicLink, new("Send Magic Link") },
            {
                SendMagicLinkSuccess,
                new(
                    "If this email matches an account an email will have been sent with your magic login link."
                )
            },
            { SendResetPwdLink, new("Send Reset Password Link") },
            {
                SendResetPwdLinkSuccess,
                new(
                    "If this email matches an account an email will have been sent to reset your password."
                )
            },
            { SignIn, new("Sign In") },
            { SignOut, new("Sign Out") },
            { SigningIn, new("Signing In") },
            { SigningOut, new("Signing Out") },
            { Success, new("Success") },
            { ThousandsSeparator, new("Thousands Separator") },
            { TimeFmt, new("Time Format") },
            { ToggleLiveUpdates, new("Toggle live updates") },
            { UnexpectedError, new("An unexpected error occurred") },
            { Update, new("Update") },
            { Updating, new("Updating") },
            { VerifyEmail, new("Verify Email") },
            { VerifyEmailSuccess, new("Thank you for verifying your email.") },
            { Verifying, new("Verifying") },
            { VerifyingEmail, new("Verifying your email") },
            { Yes, new("Yes") },
        };
}
