// Generated Code File, Do Not Edit.
// This file is generated with Common.Cli.
// see https://github.com/0xor1/common/blob/main/Common.Cli/I18n.cs
// executed with arguments: i18n <abs_file_path_to>/I18n Common.Shared.I18n true cmn_

using Common.Shared;

namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<string, TemplatableString> FR_Strings =
        new Dictionary<string, TemplatableString>()
        {
            { ApiError, new("Erreur API") },
            {
                AuthAccountNotVerified,
                new(
                    "Compte non vérifié, veuillez vérifier vos e-mails pour le lien de vérification"
                )
            },
            { AuthAlreadyAuthenticated, new("Déjà en session authentifiée") },
            {
                AuthAttemptRateLimit,
                new(
                    "Les tentatives d'authentification ne peuvent pas être effectuées plus fréquemment que toutes les {{Seconds}} secondes"
                )
            },
            {
                AuthConfirmEmailHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/verify_email?email={{Email}}&code={{Code}}\">Veuillez cliquer sur ce lien pour vérifier votre adresse e-mail</a></div>"
                )
            },
            { AuthConfirmEmailSubject, new("Confirmez votre adresse email") },
            {
                AuthConfirmEmailText,
                new(
                    "Veuillez utiliser ce lien pour vérifier votre adresse e-mail: {{BaseHref}}/cmn/auth/verify_email?email={{Email}}&code={{Code}}"
                )
            },
            { AuthFcmMessageInvalid, new("Message Fcm invalide") },
            { AuthFcmNotEnabled, new("FCM non activé") },
            { AuthFcmTokenInvalid, new("Jeton Fcm invalide") },
            { AuthFcmTopicInvalid, new("Sujet Fcm invalide Min: {{Min}}, Max: {{Max}}") },
            { AuthInvalidEmail, new("Email invalide") },
            { AuthInvalidEmailCode, new("Code e-mail invalide") },
            { AuthInvalidPwd, new("Mot de passe incorrect") },
            { AuthInvalidResetPwdCode, new("Code de mot de passe de réinitialisation invalide") },
            { AuthLessThan8Chars, new("Moins de 8 caractères") },
            {
                AuthMagicLinkHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}\">Veuillez cliquer sur ce lien pour vous connecter</a ></div>"
                )
            },
            { AuthMagicLinkSubject, new("Lien de connexion magique") },
            {
                AuthMagicLinkText,
                new(
                    "Veuillez cliquer sur ce lien pour vous connecter : {{BaseHref}}/cmn/auth/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}"
                )
            },
            { AuthNoDigit, new("Aucun chiffre") },
            { AuthNoLowerCaseChar, new("Pas de caractère minuscule") },
            { AuthNoSpecialChar, new("Aucun caractère spécial") },
            { AuthNoUpperCaseChar, new("Pas de caractère majuscule") },
            { AuthNotAuthenticated, new("Pas en session authentifiée") },
            {
                AuthResetPwdHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/reset_pwd?email={{Email}}&code={{Code}}\">Veuillez cliquer sur ce lien pour réinitialiser votre mot de passe</a></div>"
                )
            },
            { AuthResetPwdSubject, new("Réinitialiser le mot de passe") },
            {
                AuthResetPwdText,
                new(
                    "Veuillez cliquer sur ce lien pour réinitialiser votre mot de passe: {{BaseHref}}/cmn/auth/reset_pwd?email={{Email}}&code={{Code}}"
                )
            },
            {
                AuthThousandsAndDecimalSeparatorsMatch,
                new("Les milliers et les caractères séparateurs décimaux correspondent")
            },
            { BadRequest, new("Mauvaise demande") },
            { ConfirmPwd, new("Confirmez le mot de passe") },
            { DateFmt, new("Format de date") },
            { DecimalSeparator, new("Séparateur décimal") },
            { DeleteAccount, new("Supprimer le compte") },
            { DeleteAccountSuccess, new("Votre compte a été supprimé.") },
            {
                DeleteAccountWarning,
                new(
                    "Cela supprimera votre compte et toutes les données qui y sont associées. Voulez-vous toujours supprimer votre compte ?"
                )
            },
            { Deleting, new("Suppression") },
            { Demo, new("Démo") },
            {
                DemoTitle,
                new(
                    "Cette application est uniquement destinée à des fins de démonstration.\nToutes les données peuvent être effacées à tout moment et sans avertissement."
                )
            },
            { Email, new("E-mail") },
            { EntityNotFound, new("Entité introuvable") },
            { Error, new("Erreur") },
            { FileTooLarge, new("Fichier trop volumineux, limite {{MaxSize}}") },
            { InsufficientPermission, new("Permission insuffisante") },
            { Invalid, new("Invalide") },
            { L10n, new("Localisation") },
            { Language, new("Langue") },
            { Live, new("En direct:") },
            { LoadingSession, new("Session de chargement") },
            { MagicLink, new("Lien magique") },
            { MagicLinkSignIn, new("Lien magique Se connecter") },
            { MinMaxNullArgs, new("Les arguments min et max sont nuls") },
            {
                MinMaxReversedArgs,
                new("Les valeurs Min {{Min}} et Max {{Max}} ne sont pas ordonnées")
            },
            { No, new("Non") },
            { NotFound, new("Pas trouvé") },
            { NothingAtAddress, new("Désolé, rien à cette adresse.") },
            { Off, new("Désactivé") },
            { On, new("Sur") },
            { Or, new("Ou") },
            { Processing, new("Traitement") },
            { Pwd, new("Mot de passe") },
            { PwdsDontMatch, new("Les mots de passe ne correspondent pas") },
            { Register, new("Enregistrer") },
            {
                RegisterSuccess,
                new(
                    "Veuillez vérifier vos e-mails pour un lien de confirmation pour terminer l'inscription."
                )
            },
            { Registering, new("Enregistrement") },
            { RememberMe, new("Souviens-toi de moi") },
            {
                RequestBodyTooLarge,
                new("Corps de la requête trop volumineux, limitez {{MaxSize}} octets")
            },
            { ResetPwd, new("Réinitialiser le mot de passe") },
            {
                ResetPwdSuccess,
                new("Vous pouvez maintenant vous connecter avec votre nouveau mot de passe.")
            },
            { Resetting, new("Réinitialisation") },
            { RpcUnknownEndpoint, new("Point de terminaison RPC inconnu") },
            { Send, new("Envoyer") },
            { SendMagicLink, new("Envoyer un lien magique") },
            {
                SendMagicLinkSuccess,
                new(
                    "Si cet email correspond à un compte, un email vous aura été envoyé avec votre lien de connexion magique."
                )
            },
            { SendResetPwdLink, new("Envoyer le lien de réinitialisation du mot de passe") },
            {
                SendResetPwdLinkSuccess,
                new(
                    "Si cet e-mail correspond à un compte, un e-mail vous aura été envoyé pour réinitialiser votre mot de passe."
                )
            },
            { SignIn, new("S'identifier") },
            { SignOut, new("Se déconnecter") },
            { SigningIn, new("Connectez-vous") },
            { SigningOut, new("Déconnecter") },
            { Success, new("Succès") },
            { ThousandsSeparator, new("Séparateur de milliers") },
            { TimeFmt, new("Format de l'heure") },
            { ToggleLiveUpdates, new("Basculer les mises à jour en direct") },
            { UnexpectedError, new("Une erreur inattendue est apparue") },
            { Update, new("Mise à jour") },
            { Updating, new("Mise à jour") },
            { VerifyEmail, new("Vérifier les courriels") },
            { VerifyEmailSuccess, new("Merci d'avoir vérifié votre adresse e-mail.") },
            { Verifying, new("Vérification") },
            { VerifyingEmail, new("Vérification de votre e-mail") },
            { Yes, new("Oui") },
        };
}
