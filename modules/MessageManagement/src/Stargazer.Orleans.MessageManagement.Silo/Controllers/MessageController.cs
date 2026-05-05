using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Authorization;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;

namespace Stargazer.Orleans.MessageManagement.Silo.Controllers;

/// <summary>
/// 消息管理控制器
/// 提供消息发送、查询，重试和取消的API接口
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/message")]
[Authorize]
public class MessageController(IClusterClient client, ILogger<MessageController> logger) : ControllerBase
{
    private IMessageGrain GetMessageGrain() => client.GetGrain<IMessageGrain>(0);

    /// <summary>
    /// 发送单条消息
    /// </summary>
    /// <param name="input">消息输入，包含接收者、内容或模板代码</param>
    /// <returns>发送结果</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.Send}")]
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageRecordDto))]
    public async Task<MessageRecordDto> SendAsync([FromBody] SendMessageInputDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Receiver))
        {
            throw new ArgumentException("invalid_receiver");
        }

        if (string.IsNullOrWhiteSpace(input.Content) && string.IsNullOrWhiteSpace(input.TemplateCode))
        {
            throw new ArgumentException("invalid_content");
        }

        var grain = GetMessageGrain();
        return await grain.SendAsync(input);
    }

    /// <summary>
    /// 批量发送消息
    /// </summary>
    /// <param name="input">批量消息输入，包含接收者列表、内容或模板代码</param>
    /// <returns>批量发送结果</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.Send}")]
    [HttpPost("batch-send")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MessageRecordDto>))]
    public async Task<List<MessageRecordDto>> BatchSendAsync([FromBody] BatchSendMessageInputDto input)
    {
        if (input.Receivers == null || input.Receivers.Count == 0)
        {
            throw new ArgumentException("invalid_receivers");
        }

        if (string.IsNullOrWhiteSpace(input.Content) && string.IsNullOrWhiteSpace(input.TemplateCode))
        {
            throw new ArgumentException("invalid_content");
        }

        var grain = GetMessageGrain();
        return await grain.BatchSendAsync(input);
    }

    /// <summary>
    /// 根据ID获取消息记录
    /// </summary>
    /// <param name="id">消息记录GUID</param>
    /// <returns>消息记录详情</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.View}")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageRecordDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<MessageRecordDto> GetRecordAsync(Guid id)
    {
        var grain = GetMessageGrain();
        var result = await grain.GetRecordAsync(id);

        if (result == null)
        {
            throw new KeyNotFoundException("record_not_found");
        }

        return result;
    }

    /// <summary>
    /// 获取分页消息记录列表
    /// </summary>
    /// <param name="channel">按消息渠道筛选</param>
    /// <param name="status">按消息状态筛选</param>
    /// <param name="receiver">按接收者筛选</param>
    /// <param name="page">页码（默认1）</param>
    /// <param name="pageSize">每页数量（默认20）</param>
    /// <returns>分页消息记录列表</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.View}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<MessageRecordDto>))]
    public async Task<PageResult<MessageRecordDto>> GetRecordsAsync(
        [FromQuery] MessageChannel? channel,
        [FromQuery] MessageStatus? status,
        [FromQuery] string? receiver,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var grain = GetMessageGrain();
        return await grain.GetRecordsAsync(
            channel?.ToString(),
            status?.ToString(),
            receiver,
            page,
            pageSize);
    }

    /// <summary>
    /// 重试发送失败的消息
    /// </summary>
    /// <param name="id">消息记录GUID</param>
    /// <returns>重试结果</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.Retry}")]
    [HttpPost("{id:guid}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageRecordDto))]
    public async Task<MessageRecordDto> RetryAsync(Guid id)
    {
        var grain = GetMessageGrain();
        return await grain.RetryAsync(id);
    }

    /// <summary>
    /// 取消待发送的消息
    /// </summary>
    /// <param name="id">消息记录GUID</param>
    /// <returns>取消结果</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.Cancel}")]
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseData))]
    public async Task CancelAsync(Guid id)
    {
        var grain = GetMessageGrain();
        var result = await grain.CancelAsync(id);

        if (!result)
        {
            throw new InvalidOperationException("cancel_failed");
        }
    }
}
