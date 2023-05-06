using System.Text;
using Amazon.S3;
using Common.Server.Auth;
using Common.Shared;
using Common.Shared.Auth;

namespace Common.Server.Test;

public class StoreClientTests : IDisposable
{
    private readonly RpcTestRig<CommonTestDb, Api> _rpcTestRig;

    public StoreClientTests()
    {
        _rpcTestRig = new RpcTestRig<CommonTestDb, Api>(
            S.Inst,
            new AuthEps<CommonTestDb>(0, (db, s) => Task.CompletedTask).Eps,
            client => new Api(client)
        );
    }

    [Fact]
    public async Task BasicUploadAndDownload_Success()
    {
        var sc = _rpcTestRig.Get<IStoreClient>();
        var bucket = "common-test";
        await sc.CreateBucket(bucket, S3CannedACL.Private);
        // test calling create bucket doesnt throw on duplication.
        await sc.CreateBucket(bucket, S3CannedACL.Private);

        var test = "yolo baby!";
        using var us = new MemoryStream(Encoding.UTF8.GetBytes(test));
        var key = Id.New();
        await sc.Upload(bucket, key, us);
        await using var ds = await sc.Download(bucket, key);
        using var sr = new StreamReader(ds);
        var res = await sr.ReadToEndAsync();
        Assert.Equal(test, res);
    }

    public void Dispose()
    {
        _rpcTestRig.Dispose();
    }
}
