using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.Users.Grains.Abstractions.Users;
using Stargazer.Orleans.Users.Grains.Abstractions.Users.Dtos;

namespace Stargazer.Orleans.Users.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/user")]
public class UserController(IClusterClient client, ILogger<UserController> logger) : ControllerBase
{

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
    {
        if (User.Identity?.Name != null)        {
            Guid id = Guid.Parse(User.Identity.Name);
            var userGrain = client.GetGrain<IUserGrain>(0);
            var user = await userGrain.GetUserDataAsync(id, cancellationToken);
            return Ok(user);
        }
        else
        {
            return BadRequest("User identity is not available.");
        }
    }
    
    [Authorize]
    [HttpPost("update")]
    public async Task<IActionResult> Update(CreateOrUpdateUserInputDto input, CancellationToken cancellationToken = default)
    {
        if (User.Identity?.Name != null)
        {
            Guid id = Guid.Parse(User.Identity.Name);
            var userGrain = client.GetGrain<IUserGrain>(0);
            await userGrain.UpdateUserAsync(id, input, cancellationToken);
            return Ok();
        }
        else
        {
            return BadRequest("User identity is not available.");
        }
    }
}
