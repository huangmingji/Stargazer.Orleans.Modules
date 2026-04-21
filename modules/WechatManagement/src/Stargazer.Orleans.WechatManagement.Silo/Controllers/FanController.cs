using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;
using Stargazer.Orleans.WechatManagement.Silo.Authorization;

namespace Stargazer.Orleans.WechatManagement.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/wechat/{accountId}/fans")]
[Authorize]
public class FanController(IClusterClient client, ILogger<FanController> logger) : ControllerBase
{
    [HttpGet]
    [Authorize(policy: WechatPolicyNames.ViewFans)]
    public async Task<IActionResult> GetFans(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? subscribeStatus = null,
        CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatUserGrain>(0);
        var (items, total) = await grain.GetFansAsync(accountId, page, pageSize, subscribeStatus, cancellationToken);

        return Ok(new
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items
        });
    }

    [HttpGet("{openId}")]
    [Authorize(policy: WechatPolicyNames.ViewFans)]
    public async Task<IActionResult> GetFan(string openId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatUserGrain>(0);
        var fan = await grain.GetUserByOpenIdAsync(accountId, openId, cancellationToken);

        if (fan == null)
        {
            return NotFound(ResponseData.Fail("fan_not_found", "Fan not found."));
        }

        return Ok(fan);
    }

    [HttpPut("{openId}")]
    [Authorize(policy: WechatPolicyNames.UpdateFans)]
    public async Task<IActionResult> UpdateFan(string openId, Guid accountId, [FromBody] UpdateFanInput input, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatUserGrain>(0);
        
        var existingFan = await grain.GetUserByOpenIdAsync(accountId, openId, cancellationToken);
        if (existingFan == null)
        {
            return NotFound(ResponseData.Fail("fan_not_found", "Fan not found."));
        }

        var updateInput = new Grains.Abstractions.Users.Dtos.UpdateWechatUserInputDto
        {
            Remark = input.Remark,
            GroupId = input.GroupId
        };

        var result = await grain.UpdateUserAsync(existingFan.Id, updateInput, cancellationToken);

        return Ok(result);
    }
}

public class UpdateFanInput
{
    public string? Remark { get; set; }
    public Guid? GroupId { get; set; }
}