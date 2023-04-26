using Common.Shared;
using Radzen;

namespace Common.Client;

public static class AuthValidator
{
    public static ValidationResult EmailValidator(IRadzenFormComponent component)
    {
        return Shared.AuthValidator.Email(component.GetValue() as string ?? "");
    }

    public static ValidationResult PwdValidator(IRadzenFormComponent component)
    {
        return Shared.AuthValidator.Pwd(component.GetValue() as string ?? "");
    }
}
