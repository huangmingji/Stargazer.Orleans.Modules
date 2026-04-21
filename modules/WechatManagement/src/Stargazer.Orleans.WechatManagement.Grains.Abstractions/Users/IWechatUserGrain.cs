using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;

public interface IWechatUserGrain : IGrainWithIntegerKey
{
    Task<WechatUserDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<WechatUserDto?> GetUserByOpenIdAsync(Guid accountId, string openId, CancellationToken cancellationToken = default);
    
    Task<WechatUserDto> CreateUserAsync(CreateWechatUserInputDto input, CancellationToken cancellationToken = default);
    
    Task<WechatUserDto?> UpdateUserAsync(Guid id, UpdateWechatUserInputDto input, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<bool> SaveUserInfoAsync(
        Guid accountId,
        string openId,
        string? unionId,
        string nickname,
        int sex,
        string? province,
        string? city,
        string? country,
        string? headimgUrl,
        CancellationToken cancellationToken = default);

    Task SubscribeAsync(Guid id, string openId, int subscribeStatus, CancellationToken cancellationToken = default);
    
    Task UnSubscribeAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task AddTagAsync(Guid id, Guid tagId, CancellationToken cancellationToken = default);
    
    Task RemoveTagAsync(Guid id, Guid tagId, CancellationToken cancellationToken = default);

    Task<(List<WechatUserDto> Items, int Total)> GetFansAsync(
        Guid accountId, 
        int page, 
        int pageSize, 
        int? subscribeStatus,
        CancellationToken cancellationToken = default);
}
