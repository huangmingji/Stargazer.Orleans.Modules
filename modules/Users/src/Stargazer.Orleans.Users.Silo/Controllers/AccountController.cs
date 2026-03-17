using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.Users.Grains.Abstractions;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/account")]
public class AccountController(IClusterClient client, ILogger<AccountController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] VerifyPasswordInputDto input, CancellationToken cancellationToken)
    { 
        // 获取Grain引用
        var userGrain = client.GetGrain<IUserGrain>(0);
        if (await userGrain.VerifyPasswordAsync(input, cancellationToken))
        {
            var account = await userGrain.GetUserDataAsync(input.Name, cancellationToken);
            return Ok(account);
        }

        return BadRequest(ResponseData.Fail(code: "account_password_incorrect", message: "The account or password is incorrect."));
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterAccountInputDto input)
    {
        // 获取Grain引用
        var userGrain = client.GetGrain<IUserGrain>(0);
        if (await userGrain.AccountExistedAsync(input.Account))
        {
            return BadRequest(ResponseData.Fail(code:"account_exists", message: "The account already exists"));
        }
        if (!Regex.IsMatch(input.Password, @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$"))
        {
            return BadRequest(ResponseData.Fail(code:"password_not_standardized", message: "The password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one digit."));
        }

        var account = await userGrain.RegisterAsync(input);
        return Ok(account);
    }
}
