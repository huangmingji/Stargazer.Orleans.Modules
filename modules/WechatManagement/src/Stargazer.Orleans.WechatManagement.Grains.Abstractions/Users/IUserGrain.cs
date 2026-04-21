using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;

public interface IUserGrain : IGrainWithGuidKey
{
    Task<UserDataDto?> GetUserDataAsync(CancellationToken cancellationToken = default);
    
    Task<UserDataDto> CreateUserAsync(CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default);
    
    Task<UserDataDto?> UpdateUserAsync(CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default);
    
    Task DeleteUserAsync(CancellationToken cancellationToken = default);
}
