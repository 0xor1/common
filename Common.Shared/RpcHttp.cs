namespace Common.Shared;

public static class RpcHttp
{
    public const string QueryParam = "arg";
    public const string DataHeader = "X-Data";
    public const string ContentNameHeader = "X-Content-Name";
    public const string ContentTypeHeader = "Content-Type";
    public const string ContentLengthHeader = "Content-Length";
    public const string ContentDispositionHeader = "Content-Disposition";

    public static byte[] Serialize(object v, bool json = false) =>
        json ? Json.From(v).ToUtf8Bytes() : MsgPck.From(v);

    public static T Deserialize<T>(byte[] bs, bool json = false)
        where T : class => json ? Json.To<T>(bs.FromUtf8Bytes()) : MsgPck.To<T>(bs);

    public static bool HasStream<T>() => typeof(T).IsAssignableTo(typeof(HasStream));
}
