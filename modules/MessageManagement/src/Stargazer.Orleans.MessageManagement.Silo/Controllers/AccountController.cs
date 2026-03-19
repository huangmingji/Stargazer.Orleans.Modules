using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Users;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.MessageManagement.Host.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/account")]
public class AccountController(IClusterClient client, ILogger<AccountController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] VerifyPasswordInputDto input)
    { 
        // 获取Grain引用
        var userGrain = client.GetGrain<IAccountGrain>(input.Name);
        await userGrain.VerifyPasswordAsync(input);
        var account = await userGrain.GetAccountAsync();
        return Ok(account);
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterAccountInputDto input)
    {
        // 获取Grain引用
        var userGrain = client.GetGrain<IAccountGrain>(input.AccountName);
        var account = await userGrain.RegisterAsync(input);
        return Ok(account);
    }
}
