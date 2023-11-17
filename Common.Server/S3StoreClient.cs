using Amazon.S3;
using Amazon.S3.Model;
using Minio;
using Minio.DataModel.Args;

namespace Common.Server;

public class S3StoreClient : IStoreClient
{
    private readonly AmazonS3Client _awsS3;
    private readonly IMinioClient _minio;

    public S3StoreClient(AmazonS3Client awsS3, IMinioClient minio)
    {
        _awsS3 = awsS3;
        _minio = minio;
    }

    public async Task CreateBucket(string bucket, S3CannedACL acl, CancellationToken ctkn = default)
    {
        try
        {
            await _awsS3.PutBucketAsync(
                new PutBucketRequest { BucketName = bucket, CannedACL = acl },
                ctkn
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

    public async Task Move(
        string srcBucket,
        string dstBucket,
        string key,
        CancellationToken ctkn = default
    )
    {
        var res = await _awsS3.CopyObjectAsync(
            new CopyObjectRequest
            {
                SourceBucket = srcBucket,
                SourceKey = key,
                DestinationBucket = dstBucket,
                DestinationKey = key
            },
            ctkn
        );
        // TODO does this need to check if the object has been copied over successfully before calling delete?
        await _awsS3.DeleteObjectAsync(
            new DeleteObjectRequest { BucketName = srcBucket, Key = key },
            ctkn
        );
    }

    public async Task Upload(
        string bucket,
        string key,
        string type,
        ulong size,
        Stream stream,
        CancellationToken ctkn = default
    )
    {
        await _minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(key)
                .WithContentType(type)
                .WithObjectSize((long)size)
                .WithStreamData(stream),
            ctkn
        );
        await stream.DisposeAsync();
    }

    public async Task<Stream> Download(string bucket, string key, CancellationToken ctkn = default)
    {
        var res = await _awsS3.GetObjectAsync(
            new GetObjectRequest { BucketName = bucket, Key = key },
            ctkn
        );
        return res.ResponseStream;
    }

    public async Task Delete(string bucket, string key, CancellationToken ctkn = default)
    {
        await _awsS3.DeleteObjectAsync(
            new DeleteObjectRequest { BucketName = bucket, Key = key },
            ctkn
        );
    }

    public async Task DeletePrefix(string bucket, string prefix, CancellationToken ctkn = default)
    {
        if (!prefix.EndsWith("/"))
            prefix += "/";

        var res = await _awsS3.ListObjectsV2Async(
            new ListObjectsV2Request { BucketName = bucket, Prefix = prefix },
            ctkn
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

            await _awsS3.DeleteObjectsAsync(req, ctkn);

            res = await _awsS3.ListObjectsV2Async(
                new ListObjectsV2Request { BucketName = bucket, Prefix = prefix },
                ctkn
            );
        }
    }

    public void Dispose()
    {
        _awsS3.Dispose();
        _minio.Dispose();
    }
}
