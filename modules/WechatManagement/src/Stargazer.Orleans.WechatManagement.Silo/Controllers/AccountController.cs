using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions;
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
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        var accounts = await grain.GetAllAccountsAsync(cancellationToken);
        return Ok(accounts);
    }

    [HttpGet("{id:guid}")]
    [Authorize(policy: WechatPolicyNames.ViewAccounts)]
    public async Task<IActionResult> GetAccount(Guid id, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        var account = await grain.GetAccountAsync(id, cancellationToken);

        if (account == null)
        {
            return NotFound(ResponseData.Fail("account_not_found", "Account not found."));
        }

        return Ok(account);
    }

    [HttpPost]
    [Authorize(policy: WechatPolicyNames.CreateAccounts)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateWechatAccountInputDto input, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        var account = await grain.CreateAccountAsync(input, cancellationToken);
        return Ok(account);
    }

    [HttpPut("{id:guid}")]
    [Authorize(policy: WechatPolicyNames.UpdateAccounts)]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateWechatAccountInputDto input, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        var account = await grain.UpdateAccountAsync(id, input, cancellationToken);

        if (account == null)
        {
            return NotFound(ResponseData.Fail("account_not_found", "Account not found."));
        }

        return Ok(account);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(policy: WechatPolicyNames.DeleteAccounts)]
    public async Task<IActionResult> DeleteAccount(Guid id, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatAccountGrain>(0);
        var result = await grain.DeleteAccountAsync(id, cancellationToken);

        if (!result)
        {
            return NotFound(ResponseData.Fail("account_not_found", "Account not found."));
        }

        return Ok();
    }
}
