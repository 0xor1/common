using Common.Shared;

namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<string, TemplatableString> Italian = new Dictionary<
        string,
        TemplatableString
    >()
    {
        { Demo, new("Dimostrazione") },
        {
            DemoTitle,
            new(
                "Questa app è solo a scopo dimostrativo.\nTutti i dati potrebbero essere cancellati in qualsiasi momento senza preavviso."
            )
        },
        { Invalid, new("Non valido") },
        { RpcUnknownEndpoint, new("Endpoint RPC sconosciuto") },
        { UnexpectedError, new("Si è verificato un errore imprevisto") },
        { EntityNotFound, new("Entità non trovata") },
        { InsufficientPermission, new("Autorizzazione insufficiente") },
        { ApiError, new("Errore API") },
        { MinMaxNullArgs, new("Entrambi gli argomenti min e max sono nulli") },
        { MinMaxReversedArgs, new("I valori Min {{Min}} e Max {{Max}} non sono ordinati") },
        { BadRequest, new("Brutta richiesta") },
        {
            RequestBodyTooLarge,
            new("Corpo della richiesta troppo grande, limite {{MaxSize}} byte")
        },
        { AuthInvalidEmail, new("E-mail non valido") },
        { AuthInvalidPwd, new("Password non valida") },
        { AuthLessThan8Chars, new("Meno di 8 caratteri") },
        { AuthNoLowerCaseChar, new("Nessun carattere minuscolo") },
        { AuthNoUpperCaseChar, new("Nessun carattere maiuscolo") },
        { AuthNoDigit, new("Nessuna cifra") },
        { AuthNoSpecialChar, new("Nessun carattere speciale") },
        {
            AuthThousandsAndDecimalSeparatorsMatch,
            new("Le migliaia e i caratteri separatori decimali corrispondono")
        },
        { AuthAlreadyAuthenticated, new("Già in sessione autenticata") },
        { AuthNotAuthenticated, new("Non in sessione autenticata") },
        { AuthInvalidEmailCode, new("Codice e-mail non valido") },
        { AuthInvalidResetPwdCode, new("Codice di reimpostazione della password non valido") },
        {
            AuthAccountNotVerified,
            new("Account non verificato, controlla le tue e-mail per il link di verifica")
        },
        {
            AuthAttemptRateLimit,
            new(
                "I tentativi di autenticazione non possono essere effettuati più frequentemente di ogni {{Seconds}} secondi"
            )
        },
        { AuthConfirmEmailSubject, new("Conferma l'indirizzo e-mail") },
        {
            AuthConfirmEmailHtml,
            new(
                "<div><a href=\"{{BaseHref}}/verify_email?email={{Email}}&code={{Code}}\">Fai clic su questo link per verificare il tuo indirizzo email</a></div>"
            )
        },
        {
            AuthConfirmEmailText,
            new(
                "Utilizza questo link per verificare il tuo indirizzo email: {{BaseHref}}/verify_email?email={{Email}}&code={{Code}}"
            )
        },
        { AuthResetPwdSubject, new("Resetta la password") },
        {
            AuthResetPwdHtml,
            new(
                "<div><a href=\"{{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}\">Fai clic su questo link per reimpostare la tua password</a></div>"
            )
        },
        {
            AuthResetPwdText,
            new(
                "Fai clic su questo link per reimpostare la tua password: {{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}"
            )
        },
        { AuthMagicLinkSubject, new("Collegamento di accesso magico") },
        {
            AuthMagicLinkHtml,
            new(
                "<div><a href=\"{{BaseHref}}/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}\">Fai clic su questo link per accedere</a </div>"
            )
        },
        {
            AuthMagicLinkText,
            new(
                "Fai clic su questo link per accedere: {{BaseHref}}/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}"
            )
        },
        { AuthFcmTopicInvalid, new("Argomento Fcm non valido Min: {{Min}}, Max: {{Max}}") },
        { AuthFcmTokenInvalid, new("Token Fcm non valido") },
        { AuthFcmNotEnabled, new("Fcm non abilitato") },
        { AuthFcmMessageInvalid, new("Messaggio Fcm non valido") },
        { L10n, new("Localizzazione") },
        { ToggleLiveUpdates, new("Attiva/disattiva gli aggiornamenti in tempo reale") },
        { Live, new("Vivo:") },
        { Or, new("O") },
        { On, new("SU") },
        { Off, new("Spento") },
        { Language, new("Lingua") },
        { DateFmt, new("Formato data") },
        { TimeFmt, new("Formato orario") },
        { ThousandsSeparator, new("Separatore delle migliaia") },
        { DecimalSeparator, new("Separatore decimale") },
        { Register, new("Registrati") },
        { Registering, new("Registrazione") },
        {
            RegisterSuccess,
            new("Controlla le tue e-mail per un link di conferma per completare la registrazione.")
        },
        { SignIn, new("Registrazione") },
        { RememberMe, new("Ricordati di me") },
        { SigningIn, new("Registrarsi") },
        { SignOut, new("Disconnessione") },
        { SigningOut, new("Disconnessione") },
        { VerifyEmail, new("Verifica Email") },
        { Verifying, new("Verifica") },
        { VerifyingEmail, new("Verifica della tua email") },
        { VerifyEmailSuccess, new("Grazie per aver verificato la tua email.") },
        { ResetPwd, new("Resetta la password") },
        { MagicLink, new("Collegamento magico") },
        { MagicLinkSignIn, new("Accedi al collegamento magico") },
        { Email, new("E-mail") },
        { Pwd, new("Parola d'ordine") },
        { ConfirmPwd, new("Conferma password") },
        { PwdsDontMatch, new("Le password non corrispondono") },
        { ResetPwdSuccess, new("Ora puoi accedere con la tua nuova password.") },
        { Resetting, new("Ripristino") },
        { SendResetPwdLink, new("Invia collegamento per reimpostare la password") },
        {
            SendResetPwdLinkSuccess,
            new(
                "Se questa e-mail corrisponde a un account, sarà stata inviata un'e-mail per reimpostare la password."
            )
        },
        { SendMagicLink, new("Invia collegamento magico") },
        {
            SendMagicLinkSuccess,
            new(
                "Se questa email corrisponde a un account, verrà inviata un'email con il tuo collegamento di accesso magico."
            )
        },
        { Processing, new("in lavorazione") },
        { Send, new("Inviare") },
        { Success, new("Successo") },
        { Error, new("Errore") },
        { Update, new("Aggiornamento") },
        { Updating, new("In aggiornamento") }
    };
}
