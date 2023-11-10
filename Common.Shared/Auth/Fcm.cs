namespace Common.Shared.Auth;

public static class Fcm
{
    public const string ClientHeaderName = "X-Fcm-Client";
    public const string TypeName = "X-Fcm-Type";
    public const string Data = "Data";
    public const string Topic = "Topic";

    public static string TopicString(IReadOnlyList<string> topic) => string.Join(":", topic);
}
