using Common.Shared;

namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<string, TemplatableString> German = new Dictionary<
        string,
        TemplatableString
    >()
    {
        { Demo, new("Demo") },
        {
            DemoTitle,
            new(
                "Diese App dient nur zu Demonstrationszwecken.\nAlle Daten können jederzeit und ohne Vorwarnung gelöscht werden."
            )
        },
        { Invalid, new("Ungültig") },
        { RpcUnknownEndpoint, new("Unbekannter RPC-Endpunkt") },
        { UnexpectedError, new("Ein unerwarteter Fehler ist aufgetreten") },
        { EntityNotFound, new("Entität nicht gefunden") },
        { InsufficientPermission, new("Unzureichende Erlaubnis") },
        { ApiError, new("API-Fehler") },
        { MinMaxNullArgs, new("Sowohl das Min- als auch das Max-Argument sind null") },
        { MinMaxReversedArgs, new("Min. {{Min}}- und Max. {{Max}}-Werte sind nicht geordnet") },
        { BadRequest, new("Ungültige Anforderung") },
        { RequestBodyTooLarge, new("Anfragetext zu groß, maximal {{MaxSize}} Byte") },
        { AuthInvalidEmail, new("Ungültige E-Mail") },
        { AuthInvalidPwd, new("Ungültiges Passwort") },
        { AuthLessThan8Chars, new("Weniger als 8 Zeichen") },
        { AuthNoLowerCaseChar, new("Kein Kleinbuchstabe") },
        { AuthNoUpperCaseChar, new("Kein Großbuchstabe") },
        { AuthNoDigit, new("Keine Ziffer") },
        { AuthNoSpecialChar, new("Kein Sonderzeichen") },
        {
            AuthThousandsAndDecimalSeparatorsMatch,
            new("Tausender- und Dezimaltrennzeichen stimmen überein")
        },
        { AuthAlreadyAuthenticated, new("Bereits in authentifizierter Sitzung") },
        { AuthNotAuthenticated, new("Nicht in authentifizierter Sitzung") },
        { AuthInvalidEmailCode, new("Ungültiger E-Mail-Code") },
        { AuthInvalidResetPwdCode, new("Ungültiger Passwort-Reset-Code") },
        {
            AuthAccountNotVerified,
            new("Konto nicht bestätigt, bitte überprüfen Sie Ihre E-Mails auf den Bestätigungslink")
        },
        {
            AuthAttemptRateLimit,
            new(
                "Authentifizierungsversuche können nicht häufiger als alle {{Seconds}} Sekunden durchgeführt werden"
            )
        },
        { AuthConfirmEmailSubject, new("e-Mail-Adresse bestätigen") },
        {
            AuthConfirmEmailHtml,
            new(
                "<div><a href=\"{{BaseHref}}/verify_email?email={{Email}}&code={{Code}}\">Bitte klicken Sie auf diesen Link, um Ihre E-Mail-Adresse zu bestätigen</a></div>"
            )
        },
        {
            AuthConfirmEmailText,
            new(
                "Bitte verwenden Sie diesen Link, um Ihre E-Mail-Adresse zu bestätigen: {{BaseHref}}/verify_email?email={{Email}}&code={{Code}}"
            )
        },
        { AuthResetPwdSubject, new("Passwort zurücksetzen") },
        {
            AuthResetPwdHtml,
            new(
                "<div><a href=\"{{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}\">klicken Sie bitte auf diesen Link, um Ihr Passwort zurückzusetzen</a></div>"
            )
        },
        {
            AuthResetPwdText,
            new(
                "Bitte klicken Sie auf diesen Link, um Ihr Passwort zurückzusetzen: {{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}"
            )
        },
        { AuthMagicLinkSubject, new("Magic-Login-Link") },
        {
            AuthMagicLinkHtml,
            new(
                "<div><a href=\"{{BaseHref}}/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}\">Bitte klicken Sie auf diesen Link, um sich anzumelden</a ></div>"
            )
        },
        {
            AuthMagicLinkText,
            new(
                "Bitte klicken Sie auf diesen Link, um sich anzumelden: {{BaseHref}}/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}"
            )
        },
        { AuthFcmTopicInvalid, new("FCM-Thema ungültig Min: {{Min}}, Max: {{Max}}") },
        { AuthFcmTokenInvalid, new("Fcm-Token ungültig") },
        { AuthFcmNotEnabled, new("FCM nicht aktiviert") },
        { AuthFcmMessageInvalid, new("Fcm-Nachricht ungültig") },
        { L10n, new("Lokalisierung") },
        { ToggleLiveUpdates, new("Live-Updates umschalten") },
        { Live, new("Direkt:") },
        { Or, new("Oder") },
        { On, new("An") },
        { Off, new("Aus") },
        { Language, new("Sprache") },
        { DateFmt, new("Datumsformat") },
        { TimeFmt, new("Zeitformat") },
        { ThousandsSeparator, new("Tausendertrennzeichen") },
        { DecimalSeparator, new("Dezimaltrennzeichen") },
        { Register, new("Registrieren") },
        { Registering, new("Registrieren") },
        {
            RegisterSuccess,
            new(
                "Bitte überprüfen Sie Ihre E-Mails auf einen Bestätigungslink, um die Registrierung abzuschließen."
            )
        },
        { SignIn, new("Anmelden") },
        { RememberMe, new("Mich erinnern") },
        { SigningIn, new("Anmelden") },
        { SignOut, new("Austragen") },
        { SigningOut, new("Abmelden") },
        { VerifyEmail, new("E-Mail bestätigen") },
        { Verifying, new("Überprüfung") },
        { VerifyingEmail, new("Überprüfung Ihrer E-Mail") },
        { VerifyEmailSuccess, new("Danke für das Verifizieren deiner E-Mail.") },
        { ResetPwd, new("Passwort zurücksetzen") },
        { MagicLink, new("Magischer Link") },
        { MagicLinkSignIn, new("Magic Link-Anmeldung") },
        { Email, new("Email") },
        { Pwd, new("Passwort") },
        { ConfirmPwd, new("Bestätige das Passwort") },
        { PwdsDontMatch, new("Passwörter stimmen nicht überein") },
        { ResetPwdSuccess, new("Sie können sich jetzt mit Ihrem neuen Passwort anmelden.") },
        { Resetting, new("Zurücksetzen") },
        { SendResetPwdLink, new("Link zum Zurücksetzen des Passworts senden") },
        {
            SendResetPwdLinkSuccess,
            new(
                "Wenn diese E-Mail mit einem Konto übereinstimmt, wurde eine E-Mail zum Zurücksetzen Ihres Passworts gesendet."
            )
        },
        { SendMagicLink, new("Senden Sie einen magischen Link") },
        {
            SendMagicLinkSuccess,
            new(
                "Wenn diese E-Mail mit einem Konto übereinstimmt, wird eine E-Mail mit Ihrem Magic-Login-Link gesendet."
            )
        },
        { Processing, new("wird bearbeitet") },
        { Send, new("Schicken") },
        { Success, new("Erfolg") },
        { Error, new("Fehler") },
        { Update, new("Aktualisieren") },
        { Updating, new("Aktualisierung") }
    };
}
