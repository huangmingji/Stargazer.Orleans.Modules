using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;

public interface IAccountGrain : IGrainWithStringKey
{
    Task<AccountDto> GetAccountAsync(CancellationToken cancellationToken = default);
    
    Task ChangePasswordAsync(ChangePasswordInputDto input, Guid modifierId, CancellationToken cancellationToken = default);

    Task VerifyPasswordAsync(VerifyPasswordInputDto input, CancellationToken cancellationToken = default);

    Task<AccountDto> RegisterAsync(RegisterAccountInputDto input, CancellationToken cancellationToken = default);
}
