using Common.Shared;

namespace Common.Server.Test;

public class RpcTestClient : IRpcClient
{
    private Session? _session;
    private Dictionary<string, string> _headers = new();
    private Func<
        string,
        Session?,
        Dictionary<string, string>,
        object,
        CancellationToken,
        Task<(Session, object)>
    > _exe;

    public RpcTestClient(
        Func<
            string,
            Session?,
            Dictionary<string, string>,
            object,
            CancellationToken,
            Task<(Session, object)>
        > exe,
        Session? session = null
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
        (_session, var res) = await _exe(rpc.Path, _session, _headers, arg, ctkn);
        return (TRes)res;
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
