using Orleans;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions;

public interface IObjectGrain : IGrainWithIntegerKey
{
    Task<UploadResultDto> UploadAsync(Guid bucketId, string key, Stream content, string contentType, Dictionary<string, string>? metadata, CancellationToken cancellationToken = default);
    
    Task<Stream?> DownloadAsync(Guid bucketId, string key, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid bucketId, string key, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(Guid bucketId, string key, CancellationToken cancellationToken = default);
    
    Task<ObjectMetadataDto?> GetMetadataAsync(Guid bucketId, string key, CancellationToken cancellationToken = default);
    
    Task<List<ObjectMetadataDto>> ListObjectsAsync(Guid bucketId, string? prefix, CancellationToken cancellationToken = default);
    
    Task<PageResult<ObjectMetadataDto>> ListObjectsAsync(Guid bucketId, string? prefix, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    
    Task<SignedUrlDto> GetSignedUrlAsync(Guid bucketId, string key, TimeSpan expiry, string method, CancellationToken cancellationToken = default);
    
    // Multipart upload
    Task<InitiateMultipartUploadResultDto> InitiateMultipartUploadAsync(Guid bucketId, string key, string contentType, Dictionary<string, string>? metadata, CancellationToken cancellationToken = default);
    
    Task<UploadPartResultDto> UploadPartAsync(Guid bucketId, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default);
    
    Task<UploadResultDto> CompleteMultipartUploadAsync(Guid bucketId, string key, string uploadId, List<PartETagDto> parts, CancellationToken cancellationToken = default);
    
    Task AbortMultipartUploadAsync(Guid bucketId, string key, string uploadId, CancellationToken cancellationToken = default);
}
