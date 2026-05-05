using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users.Dtos;
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
    public async Task<object> GetFans(
        Guid accountId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? subscribeStatus = null,
        CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatUserGrain>(0);
        var (items, total) = await grain.GetFansAsync(accountId, page, pageSize, subscribeStatus, cancellationToken);

        return new
        {
            total = total,
            page = page,
            pageSize = pageSize,
            items = items
        };
    }

    [HttpGet("{openId}")]
    [Authorize(policy: WechatPolicyNames.ViewFans)]
    public async Task<WechatUserDto> GetFan(string openId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatUserGrain>(0);
        var fan = await grain.GetUserByOpenIdAsync(accountId, openId, cancellationToken);
        return fan ?? throw new KeyNotFoundException("fan_not_found");
    }

    [HttpPut("{openId}")]
    [Authorize(policy: WechatPolicyNames.UpdateFans)]
    public async Task<object> UpdateFan(string openId, Guid accountId, [FromBody] UpdateFanInput input, CancellationToken cancellationToken = default)
    {
        var grain = client.GetGrain<IWechatUserGrain>(0);

        var existingFan = await grain.GetUserByOpenIdAsync(accountId, openId, cancellationToken);
        if (existingFan == null) throw new KeyNotFoundException("fan_not_found");

        var updateInput = new UpdateWechatUserInputDto
        {
            Remark = input.Remark,
            GroupId = input.GroupId
        };

        return await grain.UpdateUserAsync(existingFan.Id, updateInput, cancellationToken);
    }
}

public class UpdateFanInput
{
    public string? Remark { get; set; }
    public Guid? GroupId { get; set; }
}
