using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Enums;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates.Dtos;
using ResponseData = Stargazer.Orleans.MessageManagement.Grains.Abstractions.ResponseData;

namespace Stargazer.Orleans.MessageManagement.Silo.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/template")]
public class TemplateController(IClusterClient client, ILogger<TemplateController> logger) : ControllerBase
{
    private ITemplateGrain GetTemplateGrain(Guid id) => client.GetGrain<ITemplateGrain>(id);

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTemplateInputDto input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                return BadRequest(ResponseData.Fail(code: "invalid_name", message: "Template name is required."));
            }

            if (string.IsNullOrWhiteSpace(input.Code))
            {
                return BadRequest(ResponseData.Fail(code: "invalid_code", message: "Template code is required."));
            }

            if (string.IsNullOrWhiteSpace(input.ContentTemplate))
            {
                return BadRequest(ResponseData.Fail(code: "invalid_content", message: "Content template is required."));
            }

            var grain = GetTemplateGrain(Guid.NewGuid());
            var result = await grain.CreateAsync(input);
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create template");
            return StatusCode(500, ResponseData.Fail(code: "create_failed", message: ex.Message));
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateTemplateInputDto input)
    {
        try
        {
            if (input.Id == Guid.Empty)
            {
                return BadRequest(ResponseData.Fail(code: "invalid_id", message: "Template ID is required."));
            }

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                return BadRequest(ResponseData.Fail(code: "invalid_name", message: "Template name is required."));
            }

            var grain = GetTemplateGrain(input.Id);
            var result = await grain.UpdateAsync(input);
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update template");
            return StatusCode(500, ResponseData.Fail(code: "update_failed", message: ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var grain = GetTemplateGrain(id);
            await grain.DeleteAsync(id);
            return Ok(ResponseData.Success(data: true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete template {Id}", id);
            return StatusCode(500, ResponseData.Fail(code: "delete_failed", message: ex.Message));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var grain = GetTemplateGrain(id);
            var result = await grain.GetAsync(id);
            
            if (result == null)
            {
                return NotFound(ResponseData.Fail(code: "template_not_found", message: "Template not found."));
            }
            
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get template {Id}", id);
            return StatusCode(500, ResponseData.Fail(code: "get_failed", message: ex.Message));
        }
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code, [FromQuery] MessageChannelEnum channel)
    {
        try
        {
            var grain = GetTemplateGrain(Guid.NewGuid());
            var result = await grain.GetByCodeAsync(code, channel);
            
            if (result == null)
            {
                return NotFound(ResponseData.Fail(code: "template_not_found", message: "Template not found."));
            }
            
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get template by code {Code}", code);
            return StatusCode(500, ResponseData.Fail(code: "get_failed", message: ex.Message));
        }
    }

    [HttpGet("channel/{channel}")]
    public async Task<IActionResult> GetByChannelAsync(MessageChannelEnum channel)
    {
        try
        {
            var grain = GetTemplateGrain(Guid.NewGuid());
            var results = await grain.GetByChannelAsync(channel);
            return Ok(ResponseData.Success(data: results));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get templates by channel {Channel}", channel);
            return StatusCode(500, ResponseData.Fail(code: "get_failed", message: ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplatesAsync(
        [FromQuery] MessageChannelEnum? channel,
        [FromQuery] string? searchText,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var grain = GetTemplateGrain(Guid.NewGuid());
            var (items, total) = await grain.GetTemplatesAsync(channel, searchText, isActive, page, pageSize);
            return Ok(ResponseData.Success(data: new { Items = items, Total = total, Page = page, PageSize = pageSize }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get templates");
            return StatusCode(500, ResponseData.Fail(code: "get_failed", message: ex.Message));
        }
    }

    [HttpPost("{id:guid}/preview")]
    public async Task<IActionResult> PreviewAsync(Guid id, [FromBody] Dictionary<string, string>? variables)
    {
        try
        {
            var grain = GetTemplateGrain(id);
            var result = await grain.PreviewAsync(id, variables);
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to preview template {Id}", id);
            return StatusCode(500, ResponseData.Fail(code: "preview_failed", message: ex.Message));
        }
    }
}
