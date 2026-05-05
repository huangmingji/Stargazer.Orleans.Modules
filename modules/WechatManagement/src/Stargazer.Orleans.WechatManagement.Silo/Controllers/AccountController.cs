using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Accounts.Dtos;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Authorization;
using Stargazer.Orleans.WechatManagement.Silo.Authorization;

namespace Stargazer.Orleans.WechatManagement.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/wechat/accounts")]
[Authorize]
public class AccountController(IClusterClient client, ILogger<AccountController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(policy: WechatPolicyNames.ViewAccounts)]
    public async Task<List<WechatAccountDto>> GetAccounts(CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        return await grain.GetAllAccountsAsync(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [Authorize(policy: WechatPolicyNames.ViewAccounts)]
    public async Task<WechatAccountDto> GetAccount(Guid id, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        var account = await grain.GetAccountAsync(id, cancellationToken);
        return account ?? throw new KeyNotFoundException("account_not_found");
    }

    [HttpPost]
    [Authorize(policy: WechatPolicyNames.CreateAccounts)]
    public async Task<WechatAccountDto> CreateAccount([FromBody] CreateWechatAccountInputDto input, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        return await grain.CreateAccountAsync(input, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [Authorize(policy: WechatPolicyNames.UpdateAccounts)]
    public async Task<WechatAccountDto?> UpdateAccount(Guid id, [FromBody] UpdateWechatAccountInputDto input, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        var account = await grain.UpdateAccountAsync(id, input, cancellationToken);
        if (account == null) throw new KeyNotFoundException("account_not_found");
        return account;
    }

    [HttpDelete("{id:guid}")]
    [Authorize(policy: WechatPolicyNames.DeleteAccounts)]
    public async Task DeleteAccount(Guid id, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        var result = await grain.DeleteAccountAsync(id, cancellationToken);
        if (!result) throw new KeyNotFoundException("account_not_found");
    }
}
