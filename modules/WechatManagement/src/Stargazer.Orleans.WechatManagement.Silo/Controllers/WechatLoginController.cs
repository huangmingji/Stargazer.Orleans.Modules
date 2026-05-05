using Microsoft.AspNetCore.Mvc;
using Orleans;
using Stargazer.Orleans.WechatManagement.Grains;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Users;
using IUserGrain = Stargazer.Orleans.Users.Grains.Abstractions.Users.IUserGrain;

namespace Stargazer.Orleans.WechatManagement.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/wechat/{accountId}/auth")]
public class WechatLoginController(IClusterClient clusterClient) : ControllerBase
{
    [HttpGet("qrcode")]
    public async Task<object> GenerateQrCode(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = clusterClient.GetGrain<IWechatLoginGrain>(0);
        var qrCodeData = await loginGrain.GenerateQrCodeAsync(accountId, cancellationToken);

        return new { qrcode = qrCodeData };
    }

    [HttpPost("bind")]
    public async Task<object> BindLocalUser(
        Guid accountId,
        [FromBody] WechatBindRequest request,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = clusterClient.GetGrain<IWechatLoginGrain>(0);
        var (success, token, message) = await loginGrain.BindLocalUserAsync(
            accountId,
            request.OpenId,
            request.LocalUserId,
            cancellationToken);

        if (!success) throw new InvalidOperationException("bind_failed");

        return new { success, token, message };
    }

    [HttpPost("unbind")]
    public async Task<object> Unbind(
        Guid accountId,
        [FromBody] WechatUnbindRequest request,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = clusterClient.GetGrain<IWechatLoginGrain>(0);
        var (success, token, message) = await loginGrain.UnbindAsync(
            accountId,
            request.OpenId,
            cancellationToken);

        if (!success) throw new InvalidOperationException("unbind_failed");

        return new { success, message };
    }

    [HttpPost("callback")]
    public async Task<object> ProcessScanCallback(
        Guid accountId,
        [FromBody] WechatScanCallbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = clusterClient.GetGrain<IWechatLoginGrain>(0);
        var (success, token, message) = await loginGrain.ProcessScanResultAsync(
            accountId,
            request.OpenId,
            request.SceneId,
            cancellationToken);

        return new { success, token, message };
    }

    [HttpGet("status")]
    public async Task<object> GetBindingStatus(
        Guid accountId,
        [FromQuery] string openId,
        CancellationToken cancellationToken = default)
    {
        var loginGrain = clusterClient.GetGrain<IWechatLoginGrain>(0);
        var localUserId = await loginGrain.GetLocalUserIdAsync(accountId, openId, cancellationToken);

        if (localUserId.HasValue)
        {
            var userGrain = clusterClient.GetGrain<IUserGrain>(0);
            var user = await userGrain.GetUserDataAsync((Guid)localUserId, cancellationToken);
            return new { bound = true, userId = localUserId, username = user?.Name };
        }

        return new { bound = false };
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
