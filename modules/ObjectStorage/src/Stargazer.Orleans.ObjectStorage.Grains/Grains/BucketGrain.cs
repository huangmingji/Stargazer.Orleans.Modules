using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Stargazer.Orleans.ObjectStorage.Domain.Entities;
using Stargazer.Orleans.ObjectStorage.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;

namespace Stargazer.Orleans.ObjectStorage.Grains.Grains;

[StatelessWorker]
public class BucketGrain(
    IRepository<Bucket, Guid> bucketRepository,
    IRepository<BucketPolicy, Guid> policyRepository,
    ILogger<BucketGrain> logger) : Grain, IBucketGrain
{
    public async Task<BucketDto?> GetBucketAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bucket = await bucketRepository.FindAsync(id, cancellationToken);
        return bucket?.ToDto();
    }

    public async Task<BucketDto?> GetBucketByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var bucket = await bucketRepository.FindAsync(x => x.Name == name, cancellationToken);
        return bucket?.ToDto();
    }

    public async Task<BucketDto> CreateBucketAsync(BucketDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await bucketRepository.FindAsync(x => x.Name == dto.Name, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Bucket with name '{dto.Name}' already exists");
        }

        var bucket = new Bucket
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Acl = Enum.Parse<BucketAclType>(dto.Acl),
            MaxObjectSize = dto.MaxObjectSize,
            MaxObjectCount = dto.MaxObjectCount,
            OwnerId = dto.OwnerId,
            CreationTime = DateTime.UtcNow,
            IsActive = true
        };

        var created = await bucketRepository.InsertAsync(bucket, cancellationToken);
        logger.LogInformation("Created bucket {BucketName} with id {BucketId}", created.Name, created.Id);
        return created.ToDto();
    }

    public async Task<BucketDto> UpdateBucketAsync(Guid id, BucketDto dto, CancellationToken cancellationToken = default)
    {
        var bucket = await bucketRepository.GetAsync(id, cancellationToken);
        
        bucket.Description = dto.Description;
        bucket.Acl = Enum.Parse<BucketAclType>(dto.Acl);
        bucket.MaxObjectSize = dto.MaxObjectSize;
        bucket.MaxObjectCount = dto.MaxObjectCount;
        bucket.IsActive = dto.IsActive;
        bucket.LastModifyTime = DateTime.UtcNow;

        var updated = await bucketRepository.UpdateAsync(bucket, cancellationToken);
        logger.LogInformation("Updated bucket {BucketId}", id);
        return updated.ToDto();
    }

    public async Task<bool> DeleteBucketAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bucket = await bucketRepository.FindAsync(id, cancellationToken);
        if (bucket == null)
        {
            return false;
        }

        if (bucket.CurrentObjectCount > 0)
        {
            throw new InvalidOperationException("Cannot delete bucket with existing objects");
        }

        await bucketRepository.DeleteAsync(id, cancellationToken);
        logger.LogInformation("Deleted bucket {BucketId}", id);
        return true;
    }

    public async Task<List<BucketDto>> GetUserBucketsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var buckets = await bucketRepository.FindListAsync(x => x.OwnerId == userId, cancellationToken);
        return buckets.Select(x => x.ToDto()).ToList();
    }

    public async Task<bool> HasAccessPermissionAsync(Guid bucketId, Guid userId, string action, CancellationToken cancellationToken = default)
    {
        var bucket = await bucketRepository.FindAsync(bucketId, cancellationToken);
        if (bucket == null)
        {
            return false;
        }

        if (bucket.OwnerId == userId)
        {
            return true;
        }

        if (bucket.Acl == BucketAclType.PublicRead || bucket.Acl == BucketAclType.PublicReadWrite)
        {
            return action == "Read" || bucket.Acl == BucketAclType.PublicReadWrite;
        }

        var policies = await policyRepository.FindListAsync(
            x => x.BucketId == bucketId && x.Principal == userId.ToString() && x.IsActive, 
            cancellationToken);

        foreach (var policy in policies)
        {
            if (policy.Effect == EffectType.Deny && policy.Actions.Contains(action))
            {
                return false;
            }
            if (policy.Effect == EffectType.Allow && policy.Actions.Contains(action))
            {
                return true;
            }
        }

        return false;
    }
}

internal static class BucketExtensions
{
    public static BucketDto ToDto(this Bucket bucket) => new()
    {
        Id = bucket.Id,
        Name = bucket.Name,
        Description = bucket.Description,
        Acl = bucket.Acl.ToString(),
        MaxObjectSize = bucket.MaxObjectSize,
        MaxObjectCount = bucket.MaxObjectCount,
        CurrentObjectCount = bucket.CurrentObjectCount,
        CurrentStorageSize = bucket.CurrentStorageSize,
        OwnerId = bucket.OwnerId,
        IsActive = bucket.IsActive,
        CreationTime = bucket.CreationTime
    };
}
