using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleEmail.Model;

namespace Common.Server;

public interface IStoreClient
{
    public Task CreateBucket(string bucket, S3CannedACL acl);
    public Task Move(string srcBucket, string dstBucket, string key);
    public Task Upload(string bucket, string key, Stream body);
    public Task<Stream> Download(string bucket, string key);
    public Task Delete(string bucket, string key);
    public Task DeletePrefix(string bucket, string prefix);
}

public class S3StoreClient : IStoreClient
{
    private readonly AmazonS3Client _awsS3;

    public S3StoreClient(AmazonS3Client awsS3)
    {
        _awsS3 = awsS3;
    }

    public async Task CreateBucket(string bucket, S3CannedACL acl)
    {
        await _awsS3.PutBucketAsync(
            new PutBucketRequest() { BucketName = bucket, CannedACL = acl }
        );
    }

    public async Task Move(string srcBucket, string dstBucket, string key)
    {
        var res = await _awsS3.CopyObjectAsync(
            new CopyObjectRequest()
            {
                SourceBucket = srcBucket,
                SourceKey = key,
                DestinationBucket = dstBucket,
                DestinationKey = key
            }
        );
        // TODO does this need to check if the object has been copied over successfully before calling delete?
        await _awsS3.DeleteObjectAsync(
            new DeleteObjectRequest() { BucketName = srcBucket, Key = key }
        );
    }

    public async Task Upload(string bucket, string key, Stream body)
    {
        await _awsS3.PutObjectAsync(
            new PutObjectRequest()
            {
                BucketName = bucket,
                Key = key,
                InputStream = body,
                CannedACL = S3CannedACL.Private
            }
        );
        await body.DisposeAsync();
    }

    public async Task<Stream> Download(string bucket, string key)
    {
        var res = await _awsS3.GetObjectAsync(
            new GetObjectRequest() { BucketName = bucket, Key = key }
        );
        return res.ResponseStream;
    }

    public async Task Delete(string bucket, string key)
    {
        await _awsS3.DeleteObjectAsync(
            new DeleteObjectRequest() { BucketName = bucket, Key = key }
        );
    }

    public async Task DeletePrefix(string bucket, string prefix)
    {
        if (!prefix.EndsWith("/"))
        {
            prefix += "/";
        }

        var res = await _awsS3.ListObjectsV2Async(
            new ListObjectsV2Request() { BucketName = bucket, Prefix = prefix }
        );
        while (res.MaxKeys > 0)
        {
            var req = new DeleteObjectsRequest()
            {
                BucketName = bucket,
                Objects = new List<KeyVersion>()
            };
            foreach (var obj in res.S3Objects)
            {
                req.AddKey(obj.Key);
            }

            await _awsS3.DeleteObjectsAsync(req);

            res = await _awsS3.ListObjectsV2Async(
                new ListObjectsV2Request() { BucketName = bucket, Prefix = prefix }
            );
        }
    }
}
