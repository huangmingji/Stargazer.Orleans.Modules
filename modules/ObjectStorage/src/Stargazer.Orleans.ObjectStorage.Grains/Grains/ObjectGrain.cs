using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Stargazer.Orleans.ObjectStorage.Domain.Entities;
using Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;
using BucketEntity = Stargazer.Orleans.ObjectStorage.Domain.Entities.Bucket;
using MultipartUploadEntity = Stargazer.Orleans.ObjectStorage.Domain.Entities.MultipartUpload;
using ObjectInfoEntity = Stargazer.Orleans.ObjectStorage.Domain.Entities.ObjectInfo;

namespace Stargazer.Orleans.ObjectStorage.Grains.Grains;

[StatelessWorker]
public class ObjectGrain(
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
            throw new ArgumentException("Object key cannot be empty or whitespace", nameof(key));
        }

        if (key.StartsWith("/") || key.StartsWith("\\"))
        {
            throw new ArgumentException("Object key cannot start with a forward or backward slash", nameof(key));
        }

        if (key.Contains(".."))
        {
            throw new ArgumentException("Object key cannot contain path traversal sequences (..)", nameof(key));
        }

        if (key.ContainsAny(InvalidKeyChars))
        {
            throw new ArgumentException("Object key contains invalid characters", nameof(key));
        }

        if (key.Length > 1024)
        {
            throw new ArgumentException("Object key cannot exceed 1024 characters", nameof(key));
        }
    }

    public async Task<UploadResultDto> UploadAsync(Guid bucketId, string key, Stream content, string contentType, Dictionary<string, string>? metadata, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            throw new InvalidOperationException("Bucket not found");
        }

        var existingObject = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key, cancellationToken);
        long originalSize = existingObject?.Size ?? 0;

        if (content.Length > bucket.MaxObjectSize)
        {
            throw new InvalidOperationException($"Object size exceeds the maximum allowed size of {bucket.MaxObjectSize} bytes");
        }

        if (existingObject == null && bucket.CurrentObjectCount >= bucket.MaxObjectCount)
        {
            throw new InvalidOperationException($"Bucket has reached the maximum object count limit of {bucket.MaxObjectCount}");
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

        bucket.CurrentObjectCount = await objectRepository.CountAsync(x => x.BucketId == bucketId && !x.IsDeleted, cancellationToken);
        bucket.CurrentStorageSize = bucket.CurrentStorageSize - originalSize + objectMetadata.ContentLength;
        await bucketRepository.UpdateAsync(bucket, cancellationToken);

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
        if (bucket == null)
        {
            return null;
        }

        var exists = await storageProvider.ObjectExistsAsync(bucket.Name, key, cancellationToken);
        if (!exists)
        {
            return null;
        }

        return await storageProvider.GetObjectAsync(bucket.Name, key, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            return false;
        }

        var existingObject = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key && !x.IsDeleted, cancellationToken);
        if (existingObject == null)
        {
            return false;
        }

        await storageProvider.DeleteObjectAsync(bucket.Name, key, cancellationToken);

        existingObject.IsDeleted = true;
        existingObject.LastModified = DateTime.UtcNow;
        await objectRepository.UpdateAsync(existingObject, cancellationToken);

        bucket.CurrentObjectCount = await objectRepository.CountAsync(x => x.BucketId == bucketId && !x.IsDeleted, cancellationToken);
        bucket.CurrentStorageSize = Math.Max(0, bucket.CurrentStorageSize - existingObject.Size);
        await bucketRepository.UpdateAsync(bucket, cancellationToken);

        logger.LogInformation("Deleted object {Key} from bucket {BucketId}", key, bucketId);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            return false;
        }

        return await storageProvider.ObjectExistsAsync(bucket.Name, key, cancellationToken);
    }

    public async Task<ObjectMetadataDto?> GetMetadataAsync(Guid bucketId, string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var obj = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key && !x.IsDeleted, cancellationToken);
        if (obj == null)
        {
            return null;
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
            Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(obj.Metadata) ?? new(),
            CreationTime = obj.CreationTime
        };
    }

    public async Task<List<ObjectMetadataDto>> ListObjectsAsync(Guid bucketId, string? prefix, CancellationToken cancellationToken = default)
    {
        Expression<Func<ObjectInfoEntity, bool>> predicate = x => x.BucketId == bucketId && !x.IsDeleted;
        
        var objects = await objectRepository.FindListAsync(predicate, cancellationToken);

        if (!string.IsNullOrEmpty(prefix))
        {
            objects = objects.Where(x => x.Key.StartsWith(prefix)).ToList();
        }

        return objects.Select(x => new ObjectMetadataDto
        {
            Id = x.Id,
            Key = x.Key,
            FileName = x.FileName,
            ContentType = x.ContentType,
            Size = x.Size,
            ETag = x.ETag,
            LastModified = x.LastModified,
            Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(x.Metadata) ?? new(),
            CreationTime = x.CreationTime
        }).ToList();
    }

    public async Task<PageResult<ObjectMetadataDto>> ListObjectsAsync(Guid bucketId, string? prefix, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 1000) pageSize = 1000;

        Expression<Func<ObjectInfoEntity, bool>> predicate = x => x.BucketId == bucketId && !x.IsDeleted;
        
        var allObjects = await objectRepository.FindListAsync(predicate, cancellationToken);

        if (!string.IsNullOrEmpty(prefix))
        {
            allObjects = allObjects.Where(x => x.Key.StartsWith(prefix)).ToList();
        }

        var total = allObjects.Count;
        var pagedObjects = allObjects
            .OrderByDescending(x => x.LastModified)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PageResult<ObjectMetadataDto>
        {
            Total = total,
            Items = pagedObjects.Select(x => new ObjectMetadataDto
            {
                Id = x.Id,
                Key = x.Key,
                FileName = x.FileName,
                ContentType = x.ContentType,
                Size = x.Size,
                ETag = x.ETag,
                LastModified = x.LastModified,
                Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(x.Metadata) ?? new(),
                CreationTime = x.CreationTime
            }).ToList()
        };
    }

    public async Task<SignedUrlDto> GetSignedUrlAsync(Guid bucketId, string key, TimeSpan expiry, string method, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            throw new InvalidOperationException("Bucket not found");
        }

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

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            throw new InvalidOperationException("Bucket not found");
        }

        var existingObject = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key, cancellationToken);
        if (existingObject == null && bucket.CurrentObjectCount >= bucket.MaxObjectCount)
        {
            throw new InvalidOperationException($"Bucket has reached the maximum object count limit of {bucket.MaxObjectCount}");
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

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            throw new InvalidOperationException("Bucket not found");
        }

        var multipart = await multipartRepository.FindAsync(x => x.UploadId == uploadId, cancellationToken);
        if (multipart == null || multipart.Status != UploadStatus.InProgress)
        {
            throw new InvalidOperationException("Invalid or expired multipart upload");
        }

        var etag = await storageProvider.UploadPartAsync(bucket.Name, key, uploadId, partNumber, content, cancellationToken);

        multipart.Parts.Add(new UploadPart
        {
            PartNumber = partNumber,
            ETag = etag,
            Size = content.Length
        });
        multipart.UploadedParts = multipart.Parts.Count;
        await multipartRepository.UpdateAsync(multipart, cancellationToken);

        logger.LogInformation("Uploaded part {PartNumber} for upload {UploadId}", partNumber, uploadId);

        return new UploadPartResultDto
        {
            PartNumber = partNumber,
            ETag = etag
        };
    }

    public async Task<UploadResultDto> CompleteMultipartUploadAsync(Guid bucketId, string key, string uploadId, List<PartETagDto> parts, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            throw new InvalidOperationException("Bucket not found");
        }

        var multipart = await multipartRepository.FindAsync(x => x.UploadId == uploadId, cancellationToken);
        if (multipart == null || multipart.Status != UploadStatus.InProgress)
        {
            throw new InvalidOperationException("Invalid or expired multipart upload");
        }

        var partEtags = parts.Select(p => new PartETag
        {
            PartNumber = p.PartNumber,
            ETag = p.ETag
        }).ToList();

        var totalSize = multipart.Parts.Sum(p => p.Size);
        if (totalSize > bucket.MaxObjectSize)
        {
            throw new InvalidOperationException($"Object size exceeds the maximum allowed size of {bucket.MaxObjectSize} bytes");
        }

        await storageProvider.CompleteMultipartUploadAsync(bucket.Name, key, uploadId, partEtags, cancellationToken);

        var etag = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;

        var existingObject = await objectRepository.FindAsync(x => x.BucketId == bucketId && x.Key == key, cancellationToken);
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

        bucket.CurrentObjectCount = await objectRepository.CountAsync(x => x.BucketId == bucketId && !x.IsDeleted, cancellationToken);
        bucket.CurrentStorageSize += totalSize;
        await bucketRepository.UpdateAsync(bucket, cancellationToken);

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

        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            throw new InvalidOperationException("Bucket not found");
        }

        var multipart = await multipartRepository.FindAsync(x => x.UploadId == uploadId, cancellationToken);
        if (multipart == null)
        {
            return;
        }

        await storageProvider.AbortMultipartUploadAsync(bucket.Name, key, uploadId, cancellationToken);

        multipart.Status = UploadStatus.Aborted;
        await multipartRepository.UpdateAsync(multipart, cancellationToken);

        logger.LogInformation("Aborted multipart upload {UploadId}", uploadId);
    }
}
