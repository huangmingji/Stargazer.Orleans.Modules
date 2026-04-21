using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts;

public interface IWechatAccountGrain : IGrainWithIntegerKey
{
    Task<WechatAccountDto?> GetAccountAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<WechatAccountDto> CreateAccountAsync(CreateWechatAccountInputDto input, CancellationToken cancellationToken = default);
    
    Task<WechatAccountDto?> UpdateAccountAsync(Guid id, UpdateWechatAccountInputDto input, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<List<WechatAccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken = default);

    Task<string?> GetAccessTokenAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task SetAccessTokenAsync(Guid id, string token, DateTime expiry, CancellationToken cancellationToken = default);
    
    Task<bool> ValidateCallbackAsync(Guid id, string signature, string timestamp, string nonce, string echostr, CancellationToken cancellationToken = default);
}
