using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;

public interface IWechatUserGroupGrain : IGrainWithIntegerKey
{
    Task<WechatUserGroupDto?> GetGroupAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<WechatUserGroupDto> CreateGroupAsync(CreateWechatUserGroupInputDto input, CancellationToken cancellationToken = default);
    
    Task<WechatUserGroupDto?> UpdateGroupAsync(Guid id, UpdateWechatUserGroupInputDto input, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteGroupAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<int> GetUserCountAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task SyncFromWechatAsync(Guid id, CancellationToken cancellationToken = default);
}
