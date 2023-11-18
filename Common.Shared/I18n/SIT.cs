// Generated Code File, Do Not Edit.
// This file is generated with Common.I18nCodeGen.

namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<string, TemplatableString> IT_Strings =
        new Dictionary<string, TemplatableString>()
        {
            { ApiError, new("Errore API") },
            {
                AuthAccountNotVerified,
                new("Account non verificato, controlla le tue e-mail per il link di verifica")
            },
            { AuthAlreadyAuthenticated, new("Già in sessione autenticata") },
            {
                AuthAttemptRateLimit,
                new(
                    "I tentativi di autenticazione non possono essere effettuati più frequentemente di ogni {{Seconds}} secondi"
                )
            },
            {
                AuthConfirmEmailHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/verify_email?email={{Email}}&code={{Code}}\">Fai clic su questo link per verificare il tuo indirizzo email</a></div>"
                )
            },
            { AuthConfirmEmailSubject, new("Conferma l'indirizzo e-mail") },
            {
                AuthConfirmEmailText,
                new(
                    "Utilizza questo link per verificare il tuo indirizzo email: {{BaseHref}}/verify_email?email={{Email}}&code={{Code}}"
                )
            },
            { AuthFcmMessageInvalid, new("Messaggio Fcm non valido") },
            { AuthFcmNotEnabled, new("Fcm non abilitato") },
            { AuthFcmTokenInvalid, new("Token Fcm non valido") },
            { AuthFcmTopicInvalid, new("Argomento Fcm non valido Min: {{Min}}, Max: {{Max}}") },
            { AuthInvalidEmail, new("E-mail non valido") },
            { AuthInvalidEmailCode, new("Codice e-mail non valido") },
            { AuthInvalidPwd, new("Password non valida") },
            { AuthInvalidResetPwdCode, new("Codice di reimpostazione della password non valido") },
            { AuthLessThan8Chars, new("Meno di 8 caratteri") },
            {
                AuthMagicLinkHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}\">Fai clic su questo link per accedere</a </div>"
                )
            },
            { AuthMagicLinkSubject, new("Collegamento di accesso magico") },
            {
                AuthMagicLinkText,
                new(
                    "Fai clic su questo link per accedere: {{BaseHref}}/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}"
                )
            },
            { AuthNoDigit, new("Nessuna cifra") },
            { AuthNoLowerCaseChar, new("Nessun carattere minuscolo") },
            { AuthNoSpecialChar, new("Nessun carattere speciale") },
            { AuthNoUpperCaseChar, new("Nessun carattere maiuscolo") },
            { AuthNotAuthenticated, new("Non in sessione autenticata") },
            {
                AuthResetPwdHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}\">Fai clic su questo link per reimpostare la tua password</a></div>"
                )
            },
            { AuthResetPwdSubject, new("Resetta la password") },
            {
                AuthResetPwdText,
                new(
                    "Fai clic su questo link per reimpostare la tua password: {{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}"
                )
            },
            {
                AuthThousandsAndDecimalSeparatorsMatch,
                new("Le migliaia e i caratteri separatori decimali corrispondono")
            },
            { BadRequest, new("Brutta richiesta") },
            { ConfirmPwd, new("Conferma password") },
            { DateFmt, new("Formato data") },
            { DecimalSeparator, new("Separatore decimale") },
            { Demo, new("Dimostrazione") },
            {
                DemoTitle,
                new(
                    "Questa app è solo a scopo dimostrativo.\nTutti i dati potrebbero essere cancellati in qualsiasi momento senza preavviso."
                )
            },
            { Email, new("E-mail") },
            { EntityNotFound, new("Entità non trovata") },
            { Error, new("Errore") },
            { InsufficientPermission, new("Autorizzazione insufficiente") },
            { Invalid, new("Non valido") },
            { L10n, new("Localizzazione") },
            { Language, new("Lingua") },
            { Live, new("Vivo:") },
            { LoadingSession, new("Caricamento sessione") },
            { MagicLink, new("Collegamento magico") },
            { MagicLinkSignIn, new("Accedi al collegamento magico") },
            { MinMaxNullArgs, new("Entrambi gli argomenti min e max sono nulli") },
            { MinMaxReversedArgs, new("I valori Min {{Min}} e Max {{Max}} non sono ordinati") },
            { NotFound, new("Non trovato") },
            { NothingAtAddress, new("Mi spiace, niente a questo indirizzo.") },
            { Off, new("Spento") },
            { On, new("SU") },
            { Or, new("O") },
            { Processing, new("in lavorazione") },
            { Pwd, new("Parola d'ordine") },
            { PwdsDontMatch, new("Le password non corrispondono") },
            { Register, new("Registrati") },
            {
                RegisterSuccess,
                new(
                    "Controlla le tue e-mail per un link di conferma per completare la registrazione."
                )
            },
            { Registering, new("Registrazione") },
            { RememberMe, new("Ricordati di me") },
            {
                RequestBodyTooLarge,
                new("Corpo della richiesta troppo grande, limite {{MaxSize}} byte")
            },
            { ResetPwd, new("Resetta la password") },
            { ResetPwdSuccess, new("Ora puoi accedere con la tua nuova password.") },
            { Resetting, new("Ripristino") },
            { RpcUnknownEndpoint, new("Endpoint RPC sconosciuto") },
            { Send, new("Inviare") },
            { SendMagicLink, new("Invia collegamento magico") },
            {
                SendMagicLinkSuccess,
                new(
                    "Se questa email corrisponde a un account, verrà inviata un'email con il tuo collegamento di accesso magico."
                )
            },
            { SendResetPwdLink, new("Invia collegamento per reimpostare la password") },
            {
                SendResetPwdLinkSuccess,
                new(
                    "Se questa e-mail corrisponde a un account, sarà stata inviata un'e-mail per reimpostare la password."
                )
            },
            { SignIn, new("Registrazione") },
            { SignOut, new("Disconnessione") },
            { SigningIn, new("Registrarsi") },
            { SigningOut, new("Disconnessione") },
            { Success, new("Successo") },
            { ThousandsSeparator, new("Separatore delle migliaia") },
            { TimeFmt, new("Formato orario") },
            { ToggleLiveUpdates, new("Attiva/disattiva gli aggiornamenti in tempo reale") },
            { UnexpectedError, new("Si è verificato un errore imprevisto") },
            { Update, new("Aggiornamento") },
            { Updating, new("In aggiornamento") },
            { VerifyEmail, new("Verifica Email") },
            { VerifyEmailSuccess, new("Grazie per aver verificato la tua email.") },
            { Verifying, new("Verifica") },
            { VerifyingEmail, new("Verifica della tua email") },
        };
}
