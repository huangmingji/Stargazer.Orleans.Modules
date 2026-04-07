using Orleans;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Dtos;
using Stargazer.Orleans.ObjectStorage.Grains.Abstractions.Storage;

namespace Stargazer.Orleans.ObjectStorage.Grains.Abstractions;

public interface IBucketGrain : IGrainWithIntegerKey
{
    Task<BucketDto?> GetBucketAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<BucketDto?> GetBucketByNameAsync(string name, CancellationToken cancellationToken = default);
    
    Task<BucketDto> CreateBucketAsync(BucketDto bucket, CancellationToken cancellationToken = default);
    
    Task<BucketDto> UpdateBucketAsync(Guid id, BucketDto bucket, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteBucketAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<List<BucketDto>> GetUserBucketsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<bool> HasAccessPermissionAsync(Guid bucketId, Guid userId, string action, CancellationToken cancellationToken = default);
    
    Task<bool> IsOwnerAsync(Guid bucketId, Guid userId, CancellationToken cancellationToken = default);
}
