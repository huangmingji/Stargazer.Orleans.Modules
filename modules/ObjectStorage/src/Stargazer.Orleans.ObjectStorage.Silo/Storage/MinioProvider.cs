using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using Stargazer.Orleans.ObjectStorage.Silo.Configuration;

namespace Stargazer.Orleans.ObjectStorage.Silo.Storage;

public class MinioProvider : IStorageProvider
{
    public string ProviderName => "minio";

    public MinioProvider(MinioStorageSettings settings)
    {
        // TODO: Implement with MinIO SDK when available for .NET 10
    }

    public Task<Stream> GetObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task PutObjectAsync(string bucket, string key, Stream content, ObjectMetadata metadata, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task<bool> ObjectExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task<ObjectMetadata> GetObjectMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task<List<ObjectInfo>> ListObjectsAsync(string bucket, string prefix, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task CreateBucketAsync(string bucket, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task DeleteBucketAsync(string bucket, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task<bool> BucketExistsAsync(string bucket, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task<string> GetSignedUrlAsync(string bucket, string key, TimeSpan expiry, HttpMethod method, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task<string> InitiateMultipartUploadAsync(string bucket, string key, ObjectMetadata metadata, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task<string> UploadPartAsync(string bucket, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task CompleteMultipartUploadAsync(string bucket, string key, string uploadId, List<PartETag> parts, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");

    public Task AbortMultipartUploadAsync(string bucket, string key, string uploadId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("MinIO SDK not yet available for .NET 10");
}
