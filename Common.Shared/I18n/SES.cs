// Generated Code File, Do Not Edit.
// This file is generated with Common.Cmds.

using Common.Shared;

namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<string, TemplatableString> ES_Strings =
        new Dictionary<string, TemplatableString>()
        {
            { ApiError, new("Error de API") },
            {
                AuthAccountNotVerified,
                new(
                    "Cuenta no verificada, revise sus correos electrónicos para ver el enlace de verificación"
                )
            },
            { AuthAlreadyAuthenticated, new("Ya en sesión autenticada") },
            {
                AuthAttemptRateLimit,
                new(
                    "Los intentos de autenticación no se pueden realizar con más frecuencia que cada {{Seconds}} segundos"
                )
            },
            {
                AuthConfirmEmailHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/verify_email?email={{Email}}&code={{Code}}\">Haga clic en este enlace para verificar su dirección de correo electrónico</a></div>"
                )
            },
            { AuthConfirmEmailSubject, new("Confirmar el correo") },
            {
                AuthConfirmEmailText,
                new(
                    "Utilice este enlace para verificar su dirección de correo electrónico: {{BaseHref}}/cmn/auth/verify_email?email={{Email}}&code={{Code}}"
                )
            },
            { AuthFcmMessageInvalid, new("Mensaje fcm inválido") },
            { AuthFcmNotEnabled, new("Fcm no habilitado") },
            { AuthFcmTokenInvalid, new("Token de Fcm no válido") },
            { AuthFcmTopicInvalid, new("Tema de Fcm no válido Min: {{Min}}, Max: {{Max}}") },
            { AuthInvalidEmail, new("Email inválido") },
            { AuthInvalidEmailCode, new("Código de correo electrónico no válido") },
            { AuthInvalidPwd, new("Contraseña invalida") },
            { AuthInvalidResetPwdCode, new("Código de restablecimiento de contraseña no válido") },
            { AuthLessThan8Chars, new("Menos de 8 caracteres") },
            {
                AuthMagicLinkHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}\">Fai clic su questo link per accedere</a </div>"
                )
            },
            { AuthMagicLinkSubject, new("Collegamento di accesso magico") },
            {
                AuthMagicLinkText,
                new(
                    "Fai clic su questo link per accedere: {{BaseHref}}/cmn/auth/magic_link_sign_in?email={{Email}}&code={{Code}}&remember_me={{RememberMe}}"
                )
            },
            { AuthNoDigit, new("Sin dígito") },
            { AuthNoLowerCaseChar, new("Sin carácter en minúsculas") },
            { AuthNoSpecialChar, new("Sin carácter especial") },
            { AuthNoUpperCaseChar, new("Sin carácter en mayúscula") },
            { AuthNotAuthenticated, new("No en sesión autenticada") },
            {
                AuthResetPwdHtml,
                new(
                    "<div><a href=\"{{BaseHref}}/cmn/auth/reset_pwd?email={{Email}}&code={{Code}}\">Haga clic en este enlace para restablecer su contraseña</a></div>"
                )
            },
            { AuthResetPwdSubject, new("Restablecer la contraseña") },
            {
                AuthResetPwdText,
                new(
                    "Haga clic en este enlace para restablecer su contraseña: {{BaseHref}}/cmn/auth/reset_pwd?email={{Email}}&code={{Code}}"
                )
            },
            {
                AuthThousandsAndDecimalSeparatorsMatch,
                new("Los caracteres del separador de miles y decimales coinciden")
            },
            { BadRequest, new("Solicitud incorrecta") },
            { ConfirmPwd, new("Confirmar Contraseña") },
            { DateFmt, new("Formato de fecha") },
            { DecimalSeparator, new("Separador decimal") },
            { DeleteAccount, new("Borrar cuenta") },
            { DeleteAccountSuccess, new("Tu cuenta ha sido eliminada.") },
            {
                DeleteAccountWarning,
                new(
                    "Esto eliminará su cuenta y todos los datos asociados a ella. ¿Aún quieres eliminar tu cuenta?"
                )
            },
            { Deleting, new("Eliminando") },
            { Demo, new("Manifestación") },
            {
                DemoTitle,
                new(
                    "Esta aplicación es solo para fines de demostración.\nTodos los datos pueden borrarse en cualquier momento sin previo aviso."
                )
            },
            { Email, new("Correo electrónico") },
            { EntityNotFound, new("Entidad no encontrada") },
            { Error, new("Error") },
            { FileTooLarge, new("Archivo demasiado grande, limite {{MaxSize}}") },
            { InsufficientPermission, new("Permiso insuficiente") },
            { Invalid, new("Inválido") },
            { L10n, new("Localización") },
            { Language, new("Idioma") },
            { Live, new("Vivo:") },
            { LoadingSession, new("Cargando sesión") },
            { MagicLink, new("Collegamento magico") },
            { MagicLinkSignIn, new("Accedi al collegamento magico") },
            { MinMaxNullArgs, new("Los argumentos min y max son nulos") },
            {
                MinMaxReversedArgs,
                new("Los valores Min {{Min}} y Max {{Max}} están fuera de orden")
            },
            { No, new("No") },
            { NotFound, new("No encontrado") },
            { NothingAtAddress, new("Lo sentimos, nada en esta dirección.") },
            { Off, new("De") },
            { On, new("En") },
            { Or, new("O") },
            { Processing, new("Procesando") },
            { Pwd, new("Contraseña") },
            { PwdsDontMatch, new("Las contraseñas no coinciden") },
            { Register, new("Registro") },
            {
                RegisterSuccess,
                new(
                    "Revise sus correos electrónicos para obtener un enlace de confirmación para completar el registro."
                )
            },
            { Registering, new("Registrarse") },
            { RememberMe, new("Acuérdate de mí") },
            {
                RequestBodyTooLarge,
                new("El cuerpo de la solicitud es demasiado grande; limite {{MaxSize}} bytes")
            },
            { ResetPwd, new("Restablecer la contraseña") },
            { ResetPwdSuccess, new("Ahora puede iniciar sesión con su nueva contraseña.") },
            { Resetting, new("Restablecer") },
            { RpcUnknownEndpoint, new("Extremo de RPC desconocido") },
            { Send, new("Enviar") },
            { SendMagicLink, new("Invia collegamento magico") },
            {
                SendMagicLinkSuccess,
                new(
                    "Se questa email corrisponde a un account, verrà inviata un'email con il tuo collegamento di accesso magico."
                )
            },
            { SendResetPwdLink, new("Enviar enlace de restablecimiento de contraseña") },
            {
                SendResetPwdLinkSuccess,
                new(
                    "Si este correo electrónico coincide con una cuenta, se habrá enviado un correo electrónico para restablecer su contraseña."
                )
            },
            { SignIn, new("Iniciar sesión") },
            { SignOut, new("Desconectar") },
            { SigningIn, new("Iniciando sesión") },
            { SigningOut, new("Cerrando sesión") },
            { Success, new("Éxito") },
            { ThousandsSeparator, new("Separador de miles") },
            { TimeFmt, new("Formato de tiempo") },
            { ToggleLiveUpdates, new("Alternar actualizaciones en vivo") },
            { UnexpectedError, new("Ocurrió un error inesperado") },
            { Update, new("Actualizar") },
            { Updating, new("Actualizando") },
            { VerifyEmail, new("Verificar correo electrónico") },
            { VerifyEmailSuccess, new("Gracias por verificar tu e-mail.") },
            { Verifying, new("Verificando") },
            { VerifyingEmail, new("Verificando tu correo electrónico") },
            { Yes, new("Sí") },
        };
}
