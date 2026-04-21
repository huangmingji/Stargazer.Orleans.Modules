using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages.Dtos;
using Stargazer.Orleans.WechatManagement.Silo.Authorization;

namespace Stargazer.Orleans.WechatManagement.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/wechat/{accountId}/messages")]
[Authorize]
public class MessageController(IClusterClient client, ILogger<MessageController> logger) : ControllerBase
{
    [HttpPost("template")]
    [Authorize(policy: WechatPolicyNames.SendTemplateMessage)]
    public async Task<IActionResult> SendTemplateMessage(
        Guid accountId,
        [FromBody] SendTemplateMessageInputDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            input.AccountId = accountId;
            var producer = client.GetGrain<IWechatMessageProducerGrain>(accountId.ToString());
            var messageId = await producer.EnqueueTemplateMessageAsync(input, cancellationToken);

            return Ok(new { MessageId = messageId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "发送模板消息失败");
            return BadRequest(ResponseData.Fail("send_failed", ex.Message));
        }
    }

    [HttpPost("custom")]
    [Authorize(policy: WechatPolicyNames.SendCustomMessage)]
    public async Task<IActionResult> SendCustomMessage(
        Guid accountId,
        [FromBody] SendCustomMessageInputDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            input.AccountId = accountId;
            var producer = client.GetGrain<IWechatMessageProducerGrain>(accountId.ToString());
            var messageId = await producer.EnqueueCustomMessageAsync(input, cancellationToken);

            return Ok(new { MessageId = messageId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "发送客服消息失败");
            return BadRequest(ResponseData.Fail("send_failed", ex.Message));
        }
    }

    [HttpPost("mass")]
    [Authorize(policy: WechatPolicyNames.SendMassMessage)]
    public async Task<IActionResult> SendMassMessage(
        Guid accountId,
        [FromBody] SendMassMessageInputDto input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            input.AccountId = accountId;
            var producer = client.GetGrain<IWechatMessageProducerGrain>(accountId.ToString());
            var messageId = await producer.EnqueueMassMessageAsync(input, cancellationToken);

            return Ok(new { MessageId = messageId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "发送群发消息失败");
            return BadRequest(ResponseData.Fail("send_failed", ex.Message));
        }
    }
}
