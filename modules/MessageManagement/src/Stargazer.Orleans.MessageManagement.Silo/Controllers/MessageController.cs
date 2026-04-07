using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Authorization;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;
using ResponseData = Stargazer.Orleans.MessageManagement.Grains.Abstractions.ResponseData;

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
    public async Task<IActionResult> SendAsync([FromBody] SendMessageInputDto input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input.Receiver))
            {
                return BadRequest(ResponseData.Fail(code: "invalid_receiver", message: "Receiver is required."));
            }

            if (string.IsNullOrWhiteSpace(input.Content) && string.IsNullOrWhiteSpace(input.TemplateCode))
            {
                return BadRequest(ResponseData.Fail(code: "invalid_content", message: "Content or TemplateCode is required."));
            }

            var grain = GetMessageGrain();
            var result = await grain.SendAsync(input);
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message");
            return StatusCode(500, ResponseData.Fail(code: "send_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 批量发送消息
    /// </summary>
    /// <param name="input">批量消息输入，包含接收者列表、内容或模板代码</param>
    /// <returns>批量发送结果</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.Send}")]
    [HttpPost("batch-send")]
    public async Task<IActionResult> BatchSendAsync([FromBody] BatchSendMessageInputDto input)
    {
        try
        {
            if (input.Receivers == null || input.Receivers.Count == 0)
            {
                return BadRequest(ResponseData.Fail(code: "invalid_receivers", message: "At least one receiver is required."));
            }

            if (string.IsNullOrWhiteSpace(input.Content) && string.IsNullOrWhiteSpace(input.TemplateCode))
            {
                return BadRequest(ResponseData.Fail(code: "invalid_content", message: "Content or TemplateCode is required."));
            }

            var grain = GetMessageGrain();
            var results = await grain.BatchSendAsync(input);
            return Ok(ResponseData.Success(data: results));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to batch send messages");
            return StatusCode(500, ResponseData.Fail(code: "batch_send_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 根据ID获取消息记录
    /// </summary>
    /// <param name="id">消息记录GUID</param>
    /// <returns>消息记录详情</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.View}")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRecordAsync(Guid id)
    {
        try
        {
            var grain = GetMessageGrain();
            var result = await grain.GetRecordAsync(id);
            
            if (result == null)
            {
                return NotFound(ResponseData.Fail(code: "record_not_found", message: "Message record not found."));
            }
            
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get message record {Id}", id);
            return StatusCode(500, ResponseData.Fail(code: "get_failed", message: ex.Message));
        }
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
    public async Task<IActionResult> GetRecordsAsync(
        [FromQuery] MessageChannel? channel,
        [FromQuery] MessageStatus? status,
        [FromQuery] string? receiver,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var grain = GetMessageGrain();
            var (items, total) = await grain.GetRecordsAsync(
                channel?.ToString(),
                status?.ToString(),
                receiver,
                page,
                pageSize);
            
            return Ok(ResponseData.Success(data: new { Items = items, Total = total, Page = page, PageSize = pageSize }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get message records");
            return StatusCode(500, ResponseData.Fail(code: "get_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 重试发送失败的消息
    /// </summary>
    /// <param name="id">消息记录GUID</param>
    /// <returns>重试结果</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.Retry}")]
    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> RetryAsync(Guid id)
    {
        try
        {
            var grain = GetMessageGrain();
            var result = await grain.RetryAsync(id);
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retry message {Id}", id);
            return StatusCode(500, ResponseData.Fail(code: "retry_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 取消待发送的消息
    /// </summary>
    /// <param name="id">消息记录GUID</param>
    /// <returns>取消结果</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Messages.Cancel}")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelAsync(Guid id)
    {
        try
        {
            var grain = GetMessageGrain();
            var result = await grain.CancelAsync(id);
            
            if (!result)
            {
                return BadRequest(ResponseData.Fail(code: "cancel_failed", message: "Message cannot be cancelled or not found."));
            }
            
            return Ok(ResponseData.Success(data: true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel message {Id}", id);
            return StatusCode(500, ResponseData.Fail(code: "cancel_failed", message: ex.Message));
        }
    }
}
