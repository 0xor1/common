namespace Common.Shared.Auth;

public record ResetPwd(string Email, string Code, string NewPwd);
