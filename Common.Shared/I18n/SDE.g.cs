// Generated Code File, Do Not Edit.
// This file is generated with Common.Cli.
// see https://github.com/0xor1/common/blob/main/Common.Cli/I18n.cs
// executed with arguments: i18n <abs_file_path_to>/I18n Common.Shared.I18n true cmn_

using Common.Shared;

namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<string, TemplatableString> DE_Strings =
        new Dictionary<string, TemplatableString>()
        {
            { ApiError, new("API-Fehler") },
            {
                AuthAccountNotVerified,
                new(
                    "Konto nicht bestätigt, bitte überprüfen Sie Ihre E-Mails auf den Bestätigungslink"
                )
            },
            { AuthAlreadyAuthenticated, new("Bereits in authentifizierter Sitzung") },
            {
                AuthAttemptRateLimit,
                new(
                    "Authentifizierungsversuche können nicht häufiger als alle {{Seconds}} Sekunden durchgeführt werden"
                )
            },
            {
                AuthConfirmEmailHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/verify_email?email={{Email}}&code={{Code}}\">Bitte klicken Sie auf diesen Link, um Ihre E-Mail-Adresse zu bestätigen</a></div>"
                )
            },
            { AuthConfirmEmailSubject, new("e-Mail-Adresse bestätigen") },
            {
                AuthConfirmEmailText,
                new(
                    "Bitte verwenden Sie diesen Link, um Ihre E-Mail-Adresse zu bestätigen: {{BaseHref}}/cmn/auth/verify_email?email={{Email}}&code={{Code}}"
                )
            },
            { AuthFcmMessageInvalid, new("Fcm-Nachricht ungültig") },
            { AuthFcmNotEnabled, new("FCM nicht aktiviert") },
            { AuthFcmTokenInvalid, new("Fcm-Token ungültig") },
            { AuthFcmTopicInvalid, new("FCM-Thema ungültig Min: {{Min}}, Max: {{Max}}") },
            { AuthInvalidEmail, new("Ungültige E-Mail") },
            { AuthInvalidEmailCode, new("Ungültiger E-Mail-Code") },
            { AuthInvalidPwd, new("Ungültiges Passwort") },
            { AuthInvalidResetPwdCode, new("Ungültiger Passwort-Reset-Code") },
            { AuthLessThan8Chars, new("Weniger als 8 Zeichen") },
            {
                AuthMagicLinkHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}\">Bitte klicken Sie auf diesen Link, um sich anzumelden</a ></div>"
                )
            },
            { AuthMagicLinkSubject, new("Magic-Login-Link") },
            {
                AuthMagicLinkText,
                new(
                    "Bitte klicken Sie auf diesen Link, um sich anzumelden: {{BaseHref}}/cmn/auth/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}"
                )
            },
            { AuthNoDigit, new("Keine Ziffer") },
            { AuthNoLowerCaseChar, new("Kein Kleinbuchstabe") },
            { AuthNoSpecialChar, new("Kein Sonderzeichen") },
            { AuthNoUpperCaseChar, new("Kein Großbuchstabe") },
            { AuthNotAuthenticated, new("Nicht in authentifizierter Sitzung") },
            {
                AuthResetPwdHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/reset_pwd?email={{Email}}&code={{Code}}\">klicken Sie bitte auf diesen Link, um Ihr Passwort zurückzusetzen</a></div>"
                )
            },
            { AuthResetPwdSubject, new("Passwort zurücksetzen") },
            {
                AuthResetPwdText,
                new(
                    "Bitte klicken Sie auf diesen Link, um Ihr Passwort zurückzusetzen: {{BaseHref}}/cmn/auth/reset_pwd?email={{Email}}&code={{Code}}"
                )
            },
            {
                AuthThousandsAndDecimalSeparatorsMatch,
                new("Tausender- und Dezimaltrennzeichen stimmen überein")
            },
            { BadRequest, new("Ungültige Anforderung") },
            { ConfirmPwd, new("Bestätige das Passwort") },
            { DateFmt, new("Datumsformat") },
            { DecimalSeparator, new("Dezimaltrennzeichen") },
            { DeleteAccount, new("Konto löschen") },
            { DeleteAccountSuccess, new("Dein Account wurde gelöscht.") },
            {
                DeleteAccountWarning,
                new(
                    "Dadurch werden Ihr Konto und alle damit verbundenen Daten gelöscht. Möchten Sie Ihr Konto trotzdem löschen?"
                )
            },
            { Deleting, new("Löschen") },
            { Demo, new("Demo") },
            {
                DemoTitle,
                new(
                    "Diese App dient nur zu Demonstrationszwecken.\nAlle Daten können jederzeit und ohne Vorwarnung gelöscht werden."
                )
            },
            { Email, new("Email") },
            { EntityNotFound, new("Entität nicht gefunden") },
            { Error, new("Fehler") },
            { FileTooLarge, new("Datei zu groß, begrenzen Sie {{MaxSize}}") },
            { InsufficientPermission, new("Unzureichende Erlaubnis") },
            { Invalid, new("Ungültig") },
            { L10n, new("Lokalisierung") },
            { Language, new("Sprache") },
            { Live, new("Direkt:") },
            { LoadingSession, new("Ladesitzung") },
            { MagicLink, new("Magischer Link") },
            { MagicLinkSignIn, new("Magic Link-Anmeldung") },
            { MinMaxNullArgs, new("Sowohl das Min- als auch das Max-Argument sind null") },
            { MinMaxReversedArgs, new("Min. {{Min}}- und Max. {{Max}}-Werte sind nicht geordnet") },
            { No, new("Nein") },
            { NotFound, new("Nicht gefunden") },
            { NothingAtAddress, new("Leider ist an dieser Adresse nichts zu finden.") },
            { Off, new("Aus") },
            { On, new("An") },
            { Or, new("Oder") },
            { Processing, new("wird bearbeitet") },
            { Pwd, new("Passwort") },
            { PwdsDontMatch, new("Passwörter stimmen nicht überein") },
            { Register, new("Registrieren") },
            {
                RegisterSuccess,
                new(
                    "Bitte überprüfen Sie Ihre E-Mails auf einen Bestätigungslink, um die Registrierung abzuschließen."
                )
            },
            { Registering, new("Registrieren") },
            { RememberMe, new("Mich erinnern") },
            { RequestBodyTooLarge, new("Anfragetext zu groß, maximal {{MaxSize}} Byte") },
            { ResetPwd, new("Passwort zurücksetzen") },
            { ResetPwdSuccess, new("Sie können sich jetzt mit Ihrem neuen Passwort anmelden.") },
            { Resetting, new("Zurücksetzen") },
            { RpcUnknownEndpoint, new("Unbekannter RPC-Endpunkt") },
            { Send, new("Schicken") },
            { SendMagicLink, new("Senden Sie einen magischen Link") },
            {
                SendMagicLinkSuccess,
                new(
                    "Wenn diese E-Mail mit einem Konto übereinstimmt, wird eine E-Mail mit Ihrem Magic-Login-Link gesendet."
                )
            },
            { SendResetPwdLink, new("Link zum Zurücksetzen des Passworts senden") },
            {
                SendResetPwdLinkSuccess,
                new(
                    "Wenn diese E-Mail mit einem Konto übereinstimmt, wurde eine E-Mail zum Zurücksetzen Ihres Passworts gesendet."
                )
            },
            { SignIn, new("Anmelden") },
            { SignOut, new("Austragen") },
            { SigningIn, new("Anmelden") },
            { SigningOut, new("Abmelden") },
            { Success, new("Erfolg") },
            { ThousandsSeparator, new("Tausendertrennzeichen") },
            { TimeFmt, new("Zeitformat") },
            { ToggleLiveUpdates, new("Live-Updates umschalten") },
            { UnexpectedError, new("Ein unerwarteter Fehler ist aufgetreten") },
            { Update, new("Aktualisieren") },
            { Updating, new("Aktualisierung") },
            { VerifyEmail, new("E-Mail bestätigen") },
            { VerifyEmailSuccess, new("Danke für das Verifizieren deiner E-Mail.") },
            { Verifying, new("Überprüfung") },
            { VerifyingEmail, new("Überprüfung Ihrer E-Mail") },
            { Yes, new("Ja") },
        };
}
