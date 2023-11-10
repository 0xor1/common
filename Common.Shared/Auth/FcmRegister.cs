namespace Common.Shared.Auth;

public record FcmRegister(IReadOnlyList<string> Topic, string Token, string? Client);
