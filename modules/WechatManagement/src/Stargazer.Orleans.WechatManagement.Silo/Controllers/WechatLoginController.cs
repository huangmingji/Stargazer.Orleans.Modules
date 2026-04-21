using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Stargazer.Orleans.WechatManagement.Grains;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;
using IUserGrain = Stargazer.Orleans.Users.Grains.Abstractions.Users.IUserGrain;

namespace Stargazer.Orleans.WechatManagement.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/wechat/{accountId}/auth")]
public class WechatLoginController : ControllerBase
{
    private readonly IClusterClient _clusterClient;

    public WechatLoginController(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    [HttpGet("qrcode")]
    public async Task<IActionResult> GenerateQrCode(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = _clusterClient.GetGrain<IWechatLoginGrain>(0);
        var qrCodeData = await loginGrain.GenerateQrCodeAsync(accountId, cancellationToken);

        return Ok(new { qrcode = qrCodeData });
    }

    [HttpPost("bind")]
    public async Task<IActionResult> BindLocalUser(
        Guid accountId,
        [FromBody] WechatBindRequest request,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = _clusterClient.GetGrain<IWechatLoginGrain>(0);
        var (success, token, message) = await loginGrain.BindLocalUserAsync(
            accountId,
            request.OpenId,
            request.LocalUserId,
            cancellationToken);

        if (!success)
        {
            return BadRequest(new { success, message });
        }

        return Ok(new { success, token, message });
    }

    [HttpPost("unbind")]
    public async Task<IActionResult> Unbind(
        Guid accountId,
        [FromBody] WechatUnbindRequest request,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = _clusterClient.GetGrain<IWechatLoginGrain>(0);
        var (success, token, message) = await loginGrain.UnbindAsync(
            accountId,
            request.OpenId,
            cancellationToken);

        if (!success)
        {
            return BadRequest(new { success, message });
        }

        return Ok(new { success, message });
    }

    [HttpPost("callback")]
    public async Task<IActionResult> ProcessScanCallback(
        Guid accountId,
        [FromBody] WechatScanCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = _clusterClient.GetGrain<IWechatLoginGrain>(0);
        var (success, token, message) = await loginGrain.ProcessScanResultAsync(
            accountId,
            request.OpenId,
            request.SceneId,
            cancellationToken);

        return Ok(new { success, token, message });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetBindingStatus(
        Guid accountId,
        [FromQuery] string openId,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = _clusterClient.GetGrain<IWechatLoginGrain>(0);
        var localUserId = await loginGrain.GetLocalUserIdAsync(accountId, openId, cancellationToken);

        if (localUserId.HasValue)
        {
            var userGrain = _clusterClient.GetGrain<IUserGrain>(0);
            var user = await userGrain.GetUserDataAsync((Guid) localUserId, cancellationToken);
            return Ok(new { bound = true, userId = localUserId, username = user?.Name });
        }

        return Ok(new { bound = false });
    }
}

public class WechatBindRequest
{
    public string OpenId { get; set; } = string.Empty;
    public Guid LocalUserId { get; set; }
}

public class WechatUnbindRequest
{
    public string OpenId { get; set; } = string.Empty;
}

public class WechatScanCallbackRequest
{
    public string OpenId { get; set; } = string.Empty;
    public string SceneId { get; set; } = string.Empty;
}