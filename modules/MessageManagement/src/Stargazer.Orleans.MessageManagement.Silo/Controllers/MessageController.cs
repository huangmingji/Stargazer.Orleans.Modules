using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Enums;
using ResponseData = Stargazer.Orleans.MessageManagement.Grains.Abstractions.ResponseData;

namespace Stargazer.Orleans.MessageManagement.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/message")]
public class MessageController(IClusterClient client, ILogger<MessageController> logger) : ControllerBase
{
    private IMessageGrain GetMessageGrain() => client.GetGrain<IMessageGrain>(Guid.NewGuid());

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

    [HttpGet]
    public async Task<IActionResult> GetRecordsAsync(
        [FromQuery] MessageChannelEnum? channel,
        [FromQuery] MessageStatusEnum? status,
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
