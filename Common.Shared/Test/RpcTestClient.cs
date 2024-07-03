using Common.Shared.Auth;

namespace Common.Shared.Test;

public class RpcTestClient : IRpcClient
{
    private ISession? _session;
    private Dictionary<string, string> _headers = new();
    private Func<
        string,
        ISession?,
        Dictionary<string, string>,
        object,
        CancellationToken,
        Task<(ISession, object)>
    > _exe;

    public RpcTestClient(
        Func<
            string,
            ISession?,
            Dictionary<string, string>,
            object,
            CancellationToken,
            Task<(ISession, object)>
        > exe,
        ISession? session = null
    )
    {
        _exe = exe;
        _session = session;
    }

    public async Task<TRes> Do<TArg, TRes>(
        Rpc<TArg, TRes> rpc,
        TArg arg,
        CancellationToken ctkn = default
    )
        where TArg : class
        where TRes : class
    {
        arg = CheckSerialization(arg);
        (_session, var res) = await _exe(rpc.Path, _session, _headers, arg, ctkn);
        return CheckSerialization((TRes)res);
    }

    private T CheckSerialization<T>(T obj)
        where T : class
    {
        // internally test that the argument and result types can be de/serialized correctly, by just serializing and deserializing them
        if (typeof(T) != Nothing.Type)
        {
            if (RpcHttp.HasStream<T>())
            {
                var hasStream = (obj as HasStream).NotNull();
                var stream = hasStream.Stream;
                var objBs = RpcHttp.Serialize(obj);
                obj = RpcHttp.Deserialize<T>(objBs);
                hasStream = (obj as HasStream).NotNull();
                hasStream.Stream = stream;
            }
            else
            {
                var objBs = RpcHttp.Serialize(obj);
                obj = RpcHttp.Deserialize<T>(objBs);
            }
        }

        return obj;
    }

    public string GetUrl<TArg, TRes>(Rpc<TArg, TRes> rpc, TArg arg)
        where TArg : class
        where TRes : class
    {
        Throw.OpIf(
            RpcHttp.HasStream<TArg>(),
            "can't generate get url for an rpc whose arg has a stream"
        );
        return $"test://test.test{rpc.Path}?{RpcHttp.QueryParam}={RpcHttp.Serialize(arg).ToB64()}";
    }
}
