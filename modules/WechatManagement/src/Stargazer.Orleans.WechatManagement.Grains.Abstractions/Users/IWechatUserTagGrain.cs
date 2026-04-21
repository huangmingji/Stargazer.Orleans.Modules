using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;

public interface IWechatUserTagGrain : IGrainWithIntegerKey
{
    Task<WechatUserTagDto?> GetTagAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<WechatUserTagDto> CreateTagAsync(CreateWechatUserTagInputDto input, CancellationToken cancellationToken = default);
    
    Task<WechatUserTagDto?> UpdateTagAsync(Guid id, UpdateWechatUserTagInputDto input, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteTagAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<int> GetUserCountAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task AddUserToTagAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    
    Task RemoveUserFromTagAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    
    Task SyncFromWechatAsync(Guid id, CancellationToken cancellationToken = default);
}
