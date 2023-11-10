using Amazon.S3;

namespace Common.Server;

public interface IStoreClient : IDisposable
{
    public Task CreateBucket(string bucket, S3CannedACL acl, CancellationToken ctkn = default);
    public Task Move(
        string srcBucket,
        string dstBucket,
        string key,
        CancellationToken ctkn = default
    );
    public Task Upload(
        string bucket,
        string key,
        string type,
        ulong size,
        Stream body,
        CancellationToken ctkn = default
    );
    public Task<Stream> Download(string bucket, string key, CancellationToken ctkn = default);
    public Task Delete(string bucket, string key, CancellationToken ctkn = default);
    public Task DeletePrefix(string bucket, string prefix, CancellationToken ctkn = default);
}
