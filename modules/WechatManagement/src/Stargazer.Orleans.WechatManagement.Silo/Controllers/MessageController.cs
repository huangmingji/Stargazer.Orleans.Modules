using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<object> SendTemplateMessage(
        Guid accountId,
        [FromBody] SendTemplateMessageInputDto input,
        CancellationToken cancellationToken = default)
    {
        input.AccountId = accountId;
        var producer = client.GetGrain<IWechatMessageProducerGrain>(accountId.ToString());
        var messageId = await producer.EnqueueTemplateMessageAsync(input, cancellationToken);

        return new { MessageId = messageId };
    }

    [HttpPost("custom")]
    [Authorize(policy: WechatPolicyNames.SendCustomMessage)]
    public async Task<object> SendCustomMessage(
        Guid accountId,
        [FromBody] SendCustomMessageInputDto input,
        CancellationToken cancellationToken = default)
    {
        input.AccountId = accountId;
        var producer = client.GetGrain<IWechatMessageProducerGrain>(accountId.ToString());
        var messageId = await producer.EnqueueCustomMessageAsync(input, cancellationToken);

        return new { MessageId = messageId };
    }

    [HttpPost("mass")]
    [Authorize(policy: WechatPolicyNames.SendMassMessage)]
    public async Task<object> SendMassMessage(
        Guid accountId,
        [FromBody] SendMassMessageInputDto input,
        CancellationToken cancellationToken = default)
    {
        input.AccountId = accountId;
        var producer = client.GetGrain<IWechatMessageProducerGrain>(accountId.ToString());
        var messageId = await producer.EnqueueMassMessageAsync(input, cancellationToken);

        return new { MessageId = messageId };
    }
}
