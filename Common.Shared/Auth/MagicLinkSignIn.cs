namespace Common.Shared.Auth;

public record MagicLinkSignIn(string Email, string Code, bool RememberMe);
