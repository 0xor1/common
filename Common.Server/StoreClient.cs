using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Minio;

namespace Common.Server;

public interface IStoreClient : IDisposable
{
    public Task CreateBucket(string bucket, S3CannedACL acl);
    public Task Move(string srcBucket, string dstBucket, string key);
    public Task Upload(string bucket, string key, string type, ulong size, Stream body);
    public Task<Stream> Download(string bucket, string key);
    public Task Delete(string bucket, string key);
    public Task DeletePrefix(string bucket, string prefix);
}

public class S3StoreClient : IStoreClient
{
    private readonly AmazonS3Client _awsS3;
    private readonly IMinioClient _minio;

    public S3StoreClient(AmazonS3Client awsS3, IMinioClient minio)
    {
        _awsS3 = awsS3;
        _minio = minio;
    }

    public async Task CreateBucket(string bucket, S3CannedACL acl)
    {
        try
        {
            await _awsS3.PutBucketAsync(
                new PutBucketRequest { BucketName = bucket, CannedACL = acl }
            );
        }
        catch (Exception ex)
        {
            if (
                ex.Message
                != "Your previous request to create the named bucket succeeded and you already own it."
            )
                throw;
        }
    }

    public async Task Move(string srcBucket, string dstBucket, string key)
    {
        var res = await _awsS3.CopyObjectAsync(
            new CopyObjectRequest
            {
                SourceBucket = srcBucket,
                SourceKey = key,
                DestinationBucket = dstBucket,
                DestinationKey = key
            }
        );
        // TODO does this need to check if the object has been copied over successfully before calling delete?
        await _awsS3.DeleteObjectAsync(
            new DeleteObjectRequest { BucketName = srcBucket, Key = key }
        );
    }

    public async Task Upload(string bucket, string key, string type, ulong size, Stream stream)
    {
        await _minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithContentType(type)
                .WithObjectSize((long)size)
                .WithStreamData(stream)
        );
        await stream.DisposeAsync();
    }

    public async Task<Stream> Download(string bucket, string key)
    {
        var res = await _awsS3.GetObjectAsync(
            new GetObjectRequest { BucketName = bucket, Key = key }
        );
        return res.ResponseStream;
    }

    public async Task Delete(string bucket, string key)
    {
        await _awsS3.DeleteObjectAsync(new DeleteObjectRequest { BucketName = bucket, Key = key });
    }

    public async Task DeletePrefix(string bucket, string prefix)
    {
        if (!prefix.EndsWith("/"))
            prefix += "/";

        var res = await _awsS3.ListObjectsV2Async(
            new ListObjectsV2Request { BucketName = bucket, Prefix = prefix }
        );
        while (res.S3Objects.Count > 0)
        {
            var req = new DeleteObjectsRequest
            {
                BucketName = bucket,
                Objects = new List<KeyVersion>()
            };
            foreach (var obj in res.S3Objects)
                req.AddKey(obj.Key);

            await _awsS3.DeleteObjectsAsync(req);

            res = await _awsS3.ListObjectsV2Async(
                new ListObjectsV2Request { BucketName = bucket, Prefix = prefix }
            );
        }
    }

    public void Dispose()
    {
        _awsS3.Dispose();
        _minio.Dispose();
    }
}
