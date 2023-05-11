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
            new AuthEps<CommonTestDb>(
                0,
                (_, _, _) => Task.CompletedTask,
                (_, _, _, _) => Task.CompletedTask
            ).Eps,
            client => new Api(client),
            (sp) => Task.CompletedTask
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

    [Fact]
    public async Task Delete_Success()
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
        await sc.Delete(bucket, key);
    }

    [Fact]
    public async Task DeletePrefix_Success()
    {
        var sc = _rpcTestRig.Get<IStoreClient>();
        var bucket = "common-test";
        await sc.CreateBucket(bucket, S3CannedACL.Private);
        // test calling create bucket doesnt throw on duplication.
        await sc.CreateBucket(bucket, S3CannedACL.Private);

        var test = "yolo baby!";
        using var us = new MemoryStream(Encoding.UTF8.GetBytes(test));
        var keyA = Id.New();
        var keyB = Id.New();
        await sc.Upload(bucket, string.Join("/", keyA, keyB), us);
        await sc.DeletePrefix(bucket, keyA);
    }

    public void Dispose()
    {
        _rpcTestRig.Dispose();
    }
}