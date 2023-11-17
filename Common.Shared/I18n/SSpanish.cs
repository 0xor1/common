using Common.Shared;

namespace Common.Shared.I18n;

public static partial class S
{
    private static readonly IReadOnlyDictionary<string, TemplatableString> Spanish = new Dictionary<
        string,
        TemplatableString
    >()
    {
        { Demo, new("Manifestación") },
        {
            DemoTitle,
            new(
                "Esta aplicación es solo para fines de demostración.\nTodos los datos pueden borrarse en cualquier momento sin previo aviso."
            )
        },
        { Invalid, new("Inválido") },
        { RpcUnknownEndpoint, new("Extremo de RPC desconocido") },
        { LoadingSession, new("Cargando sesión") },
        { UnexpectedError, new("Ocurrió un error inesperado") },
        { EntityNotFound, new("Entidad no encontrada") },
        { NotFound, new("No encontrado") },
        { NothingAtAddress, new("Lo sentimos, nada en esta dirección.") },
        { InsufficientPermission, new("Permiso insuficiente") },
        { ApiError, new("Error de API") },
        { MinMaxNullArgs, new("Los argumentos min y max son nulos") },
        { MinMaxReversedArgs, new("Los valores Min {{Min}} y Max {{Max}} están fuera de orden") },
        { BadRequest, new("Solicitud incorrecta") },
        {
            RequestBodyTooLarge,
            new("El cuerpo de la solicitud es demasiado grande; limite {{MaxSize}} bytes")
        },
        { AuthInvalidEmail, new("Email inválido") },
        { AuthInvalidPwd, new("Contraseña invalida") },
        { AuthLessThan8Chars, new("Menos de 8 caracteres") },
        { AuthNoLowerCaseChar, new("Sin carácter en minúsculas") },
        { AuthNoUpperCaseChar, new("Sin carácter en mayúscula") },
        { AuthNoDigit, new("Sin dígito") },
        { AuthNoSpecialChar, new("Sin carácter especial") },
        {
            AuthThousandsAndDecimalSeparatorsMatch,
            new("Los caracteres del separador de miles y decimales coinciden")
        },
        { AuthAlreadyAuthenticated, new("Ya en sesión autenticada") },
        { AuthNotAuthenticated, new("No en sesión autenticada") },
        { AuthInvalidEmailCode, new("Código de correo electrónico no válido") },
        { AuthInvalidResetPwdCode, new("Código de restablecimiento de contraseña no válido") },
        {
            AuthAccountNotVerified,
            new(
                "Cuenta no verificada, revise sus correos electrónicos para ver el enlace de verificación"
            )
        },
        {
            AuthAttemptRateLimit,
            new(
                "Los intentos de autenticación no se pueden realizar con más frecuencia que cada {{Seconds}} segundos"
            )
        },
        { AuthConfirmEmailSubject, new("Confirmar el correo") },
        {
            AuthConfirmEmailHtml,
            new(
                "<div><a href=\"{{BaseHref}}/verify_email?email={{Email}}&code={{Code}}\">Haga clic en este enlace para verificar su dirección de correo electrónico</a></div>"
            )
        },
        {
            AuthConfirmEmailText,
            new(
                "Utilice este enlace para verificar su dirección de correo electrónico: {{BaseHref}}/verify_email?email={{Email}}&code={{Code}}"
            )
        },
        { AuthResetPwdSubject, new("Restablecer la contraseña") },
        {
            AuthResetPwdHtml,
            new(
                "<div><a href=\"{{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}\">Haga clic en este enlace para restablecer su contraseña</a></div>"
            )
        },
        {
            AuthResetPwdText,
            new(
                "Haga clic en este enlace para restablecer su contraseña: {{BaseHref}}/reset_pwd?email={{Email}}&code={{Code}}"
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
        { AuthFcmTopicInvalid, new("Tema de Fcm no válido Min: {{Min}}, Max: {{Max}}") },
        { AuthFcmTokenInvalid, new("Token de Fcm no válido") },
        { AuthFcmNotEnabled, new("Fcm no habilitado") },
        { AuthFcmMessageInvalid, new("Mensaje fcm inválido") },
        { L10n, new("Localización") },
        { ToggleLiveUpdates, new("Alternar actualizaciones en vivo") },
        { Live, new("Vivo:") },
        { Or, new("O") },
        { On, new("En") },
        { Off, new("De") },
        { Language, new("Idioma") },
        { DateFmt, new("Formato de fecha") },
        { TimeFmt, new("Formato de tiempo") },
        { ThousandsSeparator, new("Separador de miles") },
        { DecimalSeparator, new("Separador decimal") },
        { Register, new("Registro") },
        { Registering, new("Registrarse") },
        {
            RegisterSuccess,
            new(
                "Revise sus correos electrónicos para obtener un enlace de confirmación para completar el registro."
            )
        },
        { SignIn, new("Iniciar sesión") },
        { RememberMe, new("Acuérdate de mí") },
        { SigningIn, new("Iniciando sesión") },
        { SignOut, new("Desconectar") },
        { SigningOut, new("Cerrando sesión") },
        { VerifyEmail, new("Verificar correo electrónico") },
        { Verifying, new("Verificando") },
        { VerifyingEmail, new("Verificando tu correo electrónico") },
        { VerifyEmailSuccess, new("Gracias por verificar tu e-mail.") },
        { ResetPwd, new("Restablecer la contraseña") },
        { MagicLink, new("Collegamento magico") },
        { MagicLinkSignIn, new("Accedi al collegamento magico") },
        { Email, new("Correo electrónico") },
        { Pwd, new("Contraseña") },
        { ConfirmPwd, new("Confirmar Contraseña") },
        { PwdsDontMatch, new("Las contraseñas no coinciden") },
        { ResetPwdSuccess, new("Ahora puede iniciar sesión con su nueva contraseña.") },
        { Resetting, new("Restablecer") },
        { SendResetPwdLink, new("Enviar enlace de restablecimiento de contraseña") },
        {
            SendResetPwdLinkSuccess,
            new(
                "Si este correo electrónico coincide con una cuenta, se habrá enviado un correo electrónico para restablecer su contraseña."
            )
        },
        { SendMagicLink, new("Invia collegamento magico") },
        {
            SendMagicLinkSuccess,
            new(
                "Se questa email corrisponde a un account, verrà inviata un'email con il tuo collegamento di accesso magico."
            )
        },
        { Processing, new("Procesando") },
        { Send, new("Enviar") },
        { Success, new("Éxito") },
        { Error, new("Error") },
        { Update, new("Actualizar") },
        { Updating, new("Actualizando") }
    };
}
