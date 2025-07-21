using MessagePack;
using MessagePack.Resolvers;

namespace Common.Shared;

public static class MsgPck
{
    private static readonly IFormatterResolver Resolver = CompositeResolver.Create(
        MsgPackStringResolver.Instance,
        ContractlessStandardResolver.Instance
    );

    private static readonly MessagePackSerializerOptions Options =
        ContractlessStandardResolver.Options.WithResolver(Resolver);

    public static byte[] From(object v) => MessagePackSerializer.Serialize(v, Options);

    public static T To<T>(byte[] bs)
        where T : class => MessagePackSerializer.Deserialize<T>(bs, Options);
}
