using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Stargazer.Orleans.ObjectStorage.Domain.ObjectStorage;
using Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using BucketEntity = Stargazer.Orleans.ObjectStorage.Domain.ObjectStorage.Bucket;
using MultipartUploadEntity = Stargazer.Orleans.ObjectStorage.Domain.ObjectStorage.MultipartUpload;
using ObjectInfoEntity = Stargazer.Orleans.ObjectStorage.Domain.ObjectStorage.ObjectInfo;

namespace Stargazer.Orleans.ObjectStorage.Grains.Grains;

[StatelessWorker]
public partial class ObjectGrain(
    IRepository<ObjectInfoEntity, Guid> objectRepository,
    IRepository<MultipartUploadEntity, Guid> multipartRepository,
    IRepository<BucketEntity, Guid> bucketRepository,
    IStorageProvider storageProvider,
    ILogger<ObjectGrain> logger) : Grain, IObjectGrain
{
    private static readonly char[] InvalidKeyChars = { '\\', '\0' };

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("invalid_key", nameof(key));
        }

        if (key.StartsWith("/") || key.StartsWith("\\"))
        {
            throw new ArgumentException("invalid_key", nameof(key));
        }

        if (key.Contains(".."))
        {
            throw new ArgumentException("invalid_key", nameof(key));
        }

        if (key.ContainsAny(InvalidKeyChars))
        {
            throw new ArgumentException("invalid_key", nameof(key));
        }

        if (key.Length > 1024)
        {
            throw new ArgumentException("invalid_key", nameof(key));
        }
    }

    public async Task<UploadResultDto> UploadAsync(Guid bucketId, string key, Stream content, string contentType, Dictionary<string, string>? metadata, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await GetBucketOrThrowAsync(bucketId, cancellationToken);
        var existingObject = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key, cancellationToken);
        long originalSize = existingObject?.Size ?? 0;

        if (content.Length > bucket.MaxObjectSize)
        {
            throw new InvalidOperationException("object_size_exceeded");
        }

        if (existingObject == null && bucket.CurrentObjectCount >= bucket.MaxObjectCount)
        {
            throw new InvalidOperationException("bucket_object_limit_reached");
        }

        var objectMetadata = new ObjectMetadata
        {
            ContentType = contentType,
            ContentLength = content.Length,
            Metadata = metadata ?? new Dictionary<string, string>()
        };

        await storageProvider.PutObjectAsync(bucket.Name, key, content, objectMetadata, cancellationToken);

        var etag = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;

        if (existingObject != null)
        {
            existingObject.Size = objectMetadata.ContentLength;
            existingObject.ETag = etag;
            existingObject.ContentType = contentType;
            existingObject.Metadata = JsonSerializer.Serialize(objectMetadata.Metadata);
            existingObject.LastModified = now;
            await objectRepository.UpdateAsync(existingObject, cancellationToken);
        }
        else
        {
            var newObject = new ObjectInfoEntity
            {
                Id = Guid.NewGuid(),
                BucketId = bucketId,
                Key = key,
                ContentType = contentType,
                Size = objectMetadata.ContentLength,
                ETag = etag,
                Metadata = JsonSerializer.Serialize(objectMetadata.Metadata),
                LastModified = now,
                CreationTime = now
            };
            await objectRepository.InsertAsync(newObject, cancellationToken);
        }

        await UpdateBucketStatsAsync(bucketId, bucket, originalSize, objectMetadata.ContentLength, cancellationToken);

        logger.LogInformation("Uploaded object {Key} to bucket {BucketId}", key, bucketId);

        return new UploadResultDto
        {
            Key = key,
            ETag = etag,
            Size = objectMetadata.ContentLength,
            ContentType = contentType,
            LastModified = now,
            Url = $"/{bucket.Name}/{key}"
        };
    }

    public async Task<Stream?> DownloadAsync(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null) return null;

        var exists = await storageProvider.ObjectExistsAsync(bucket.Name, key, cancellationToken);
        return exists ? await storageProvider.GetObjectAsync(bucket.Name, key, cancellationToken) : null;
    }

    public async Task<bool> DeleteAsync(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null) return false;

        var existingObject = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key && !x.IsDeleted, cancellationToken);
        if (existingObject == null) return false;

        await storageProvider.DeleteObjectAsync(bucket.Name, key, cancellationToken);

        existingObject.IsDeleted = true;
        existingObject.LastModified = DateTime.UtcNow;
        await objectRepository.UpdateAsync(existingObject, cancellationToken);

        await UpdateBucketStatsAsync(bucketId, bucket, existingObject.Size, 0, cancellationToken);

        logger.LogInformation("Deleted object {Key} from bucket {BucketId}", key, bucketId);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null) return false;

        return await storageProvider.ObjectExistsAsync(bucket.Name, key, cancellationToken);
    }

    public async Task<ObjectMetadataDto?> GetMetadataAsync(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var obj = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key && !x.IsDeleted, cancellationToken);
        return obj?.ToMetadataDto();
    }

    public async Task<List<ObjectMetadataDto>> ListObjectsAsync(Guid bucketId, string? prefix, CancellationToken cancellationToken = default)
    {
        Expression<Func<ObjectInfoEntity, bool>> predicate = string.IsNullOrEmpty(prefix)
            ? x => x.BucketId == bucketId && !x.IsDeleted
            : x => x.BucketId == bucketId && !x.IsDeleted && x.Key.StartsWith(prefix);

        var objects = await objectRepository.FindListAsync(predicate, cancellationToken);
        return objects.Select(x => x.ToMetadataDto()).ToList();
    }

    public async Task<PageResult<ObjectMetadataDto>> ListObjectsAsync(Guid bucketId, string? prefix, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 1000) pageSize = 1000;

        Expression<Func<ObjectInfoEntity, bool>> predicate = string.IsNullOrEmpty(prefix)
            ? x => x.BucketId == bucketId && !x.IsDeleted
            : x => x.BucketId == bucketId && !x.IsDeleted && x.Key.StartsWith(prefix);

        var (objects, total) = await objectRepository.FindListAsync(
            predicate,
            pageIndex,
            pageSize,
            x => x.LastModified,
            true,
            cancellationToken);

        return new PageResult<ObjectMetadataDto>
        {
            Total = total,
            Items = objects.Select(x => x.ToMetadataDto()).ToList()
        };
    }

    public async Task<SignedUrlDto> GetSignedUrlAsync(Guid bucketId, string key, TimeSpan expiry, string method, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await GetBucketOrThrowAsync(bucketId, cancellationToken);
        var signedUrl = await storageProvider.GetSignedUrlAsync(bucket.Name, key, expiry, new HttpMethod(method), cancellationToken);

        return new SignedUrlDto
        {
            Url = signedUrl,
            ExpiresAt = DateTime.UtcNow.Add(expiry)
        };
    }

    public async Task<InitiateMultipartUploadResultDto> InitiateMultipartUploadAsync(Guid bucketId, string key, string contentType, Dictionary<string, string>? metadata, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await GetBucketOrThrowAsync(bucketId, cancellationToken);
        var existingObject = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key, cancellationToken);

        if (existingObject == null && bucket.CurrentObjectCount >= bucket.MaxObjectCount)
        {
            throw new InvalidOperationException("bucket_object_limit_reached");
        }

        var objectMetadata = new ObjectMetadata
        {
            ContentType = contentType,
            Metadata = metadata ?? new()
        };

        var uploadId = await storageProvider.InitiateMultipartUploadAsync(bucket.Name, key, objectMetadata, cancellationToken);

        var multipart = new MultipartUploadEntity
        {
            Id = Guid.NewGuid(),
            BucketId = bucketId,
            Key = key,
            UploadId = uploadId,
            ContentType = contentType,
            Metadata = JsonSerializer.Serialize(metadata ?? new Dictionary<string, string>()),
            Status = UploadStatus.InProgress,
            CreationTime = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await multipartRepository.InsertAsync(multipart, cancellationToken);

        logger.LogInformation("Initiated multipart upload for {Key} in bucket {BucketId}, uploadId: {UploadId}", key, bucketId, uploadId);

        return new InitiateMultipartUploadResultDto
        {
            UploadId = uploadId,
            Key = key
        };
    }

    public async Task<UploadPartResultDto> UploadPartAsync(Guid bucketId, string key, string uploadId, int partNumber, Stream content, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        await GetBucketOrThrowAsync(bucketId, cancellationToken);
        var multipart = await GetMultipartOrThrowAsync(uploadId, bucketId, key, cancellationToken);

        var etag = await storageProvider.UploadPartAsync(
            (await bucketRepository.FindAsync(bucketId, cancellationToken))!.Name,
            key, uploadId, partNumber, content, cancellationToken);

        multipart.Parts.Add(new UploadPart { PartNumber = partNumber, ETag = etag, Size = content.Length });
        multipart.UploadedParts = multipart.Parts.Count;
        await multipartRepository.UpdateAsync(multipart, cancellationToken);

        logger.LogInformation("Uploaded part {PartNumber} for upload {UploadId}", partNumber, uploadId);

        return new UploadPartResultDto { PartNumber = partNumber, ETag = etag };
    }

    public async Task<UploadResultDto> CompleteMultipartUploadAsync(Guid bucketId, string key, string uploadId, List<PartETagDto> parts, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await GetBucketOrThrowAsync(bucketId, cancellationToken);
        var multipart = await GetMultipartOrThrowAsync(uploadId, bucketId, key, cancellationToken);

        var partEtags = parts.Select(p => new PartETag { PartNumber = p.PartNumber, ETag = p.ETag }).ToList();
        var totalSize = multipart.Parts.Sum(p => p.Size);

        if (totalSize > bucket.MaxObjectSize)
        {
            throw new InvalidOperationException("object_size_exceeded");
        }

        await storageProvider.CompleteMultipartUploadAsync(bucket.Name, key, uploadId, partEtags, cancellationToken);

        var etag = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;

        var existingObject = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key, cancellationToken);
        var originalSize = existingObject?.Size ?? 0;

        if (existingObject != null)
        {
            existingObject.Size = totalSize;
            existingObject.ETag = etag;
            existingObject.LastModified = now;
            await objectRepository.UpdateAsync(existingObject, cancellationToken);
        }
        else
        {
            var newObject = new ObjectInfoEntity
            {
                Id = Guid.NewGuid(),
                BucketId = bucketId,
                Key = key,
                ContentType = multipart.ContentType,
                Size = totalSize,
                ETag = etag,
                Metadata = multipart.Metadata,
                LastModified = now,
                CreationTime = now
            };
            await objectRepository.InsertAsync(newObject, cancellationToken);
        }

        multipart.Status = UploadStatus.Completed;
        await multipartRepository.UpdateAsync(multipart, cancellationToken);

        await UpdateBucketStatsAsync(bucketId, bucket, originalSize, totalSize, cancellationToken);

        logger.LogInformation("Completed multipart upload for {Key}, uploadId: {UploadId}", key, uploadId);

        return new UploadResultDto
        {
            Key = key,
            ETag = etag,
            Size = totalSize,
            ContentType = multipart.ContentType,
            LastModified = now,
            Url = $"/{bucket.Name}/{key}"
        };
    }

    public async Task AbortMultipartUploadAsync(Guid bucketId, string key, string uploadId, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        await GetBucketOrThrowAsync(bucketId, cancellationToken);

        var multipart = await multipartRepository.FindAsync(x => x.UploadId == uploadId, cancellationToken);
        if (multipart == null) return;

        if (multipart.BucketId != bucketId || multipart.Key != key)
        {
            throw new InvalidOperationException("multipart_mismatch");
        }

        await storageProvider.AbortMultipartUploadAsync(
            (await bucketRepository.FindAsync(bucketId, cancellationToken))!.Name,
            key, uploadId, cancellationToken);

        multipart.Status = UploadStatus.Aborted;
        await multipartRepository.UpdateAsync(multipart, cancellationToken);

        logger.LogInformation("Aborted multipart upload {UploadId}", uploadId);
    }

    private async Task<BucketEntity> GetBucketOrThrowAsync(Guid bucketId, CancellationToken cancellationToken)
    {
        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            throw new KeyNotFoundException("bucket_not_found");
        }
        return bucket;
    }

    private async Task<MultipartUploadEntity> GetMultipartOrThrowAsync(string uploadId, Guid bucketId, string key, CancellationToken cancellationToken)
    {
        var multipart = await multipartRepository.FindAsync(x => x.UploadId == uploadId, cancellationToken);
        if (multipart == null || multipart.Status != UploadStatus.InProgress)
        {
            throw new InvalidOperationException("invalid_multipart");
        }

        if (multipart.BucketId != bucketId || multipart.Key != key)
        {
            throw new InvalidOperationException("multipart_mismatch");
        }

        return multipart;
    }

    private async Task UpdateBucketStatsAsync(Guid bucketId, BucketEntity bucket, long removedSize, long addedSize, CancellationToken cancellationToken)
    {
        bucket.CurrentObjectCount = await objectRepository.CountAsync(
            x => x.BucketId == bucketId && !x.IsDeleted, cancellationToken);
        bucket.CurrentStorageSize = Math.Max(0, bucket.CurrentStorageSize - removedSize + addedSize);
        await bucketRepository.UpdateAsync(bucket, cancellationToken);
    }
}

internal static class ObjectInfoExtensions
{
    public static ObjectMetadataDto ToMetadataDto(this ObjectInfoEntity obj)
    {
        Dictionary<string, string> metadata;
        try
        {
            metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(obj.Metadata) ?? new();
        }
        catch
        {
            metadata = new();
        }

        return new ObjectMetadataDto
        {
            Id = obj.Id,
            Key = obj.Key,
            FileName = obj.FileName,
            ContentType = obj.ContentType,
            Size = obj.Size,
            ETag = obj.ETag,
            LastModified = obj.LastModified,
            Metadata = metadata,
            CreationTime = obj.CreationTime
        };
    }
}
