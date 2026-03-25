using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates.Dtos;
using ResponseData = Stargazer.Orleans.MessageManagement.Grains.Abstractions.ResponseData;

namespace Stargazer.Orleans.MessageManagement.Silo.Controllers;

/// <summary>
/// 消息模板管理控制器
/// 提供模板创建、更新、删除和查询的API接口
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("api/template")]
[Authorize]
public class TemplateController(IClusterClient client, ILogger<TemplateController> logger) : ControllerBase
{
    private ITemplateGrain Grain => client.GetGrain<ITemplateGrain>(0);

    /// <summary>
    /// 创建新模板
    /// </summary>
    /// <param name="input">模板创建输入</param>
    /// <returns>创建的模板详情</returns>
    [Authorize(policy: "permission:template.create")]
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

            var result = await Grain.CreateAsync(input);
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create template");
            return StatusCode(500, ResponseData.Fail(code: "create_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 更新现有模板
    /// </summary>
    /// <param name="input">模板更新输入</param>
    /// <returns>更新后的模板详情</returns>
    [Authorize(policy: "permission:template.update")]
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

            var result = await Grain.UpdateAsync(input);
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update template");
            return StatusCode(500, ResponseData.Fail(code: "update_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 根据ID删除模板
    /// </summary>
    /// <param name="id">模板GUID</param>
    /// <returns>删除结果</returns>
    [Authorize(policy: "permission:template.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            await Grain.DeleteAsync(id);
            return Ok(ResponseData.Success(data: true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete template {Id}", id);
            return StatusCode(500, ResponseData.Fail(code: "delete_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 根据ID获取模板
    /// </summary>
    /// <param name="id">模板GUID</param>
    /// <returns>模板详情</returns>
    [Authorize(policy: "permission:template.view")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var result = await Grain.GetAsync(id);
            
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

    /// <summary>
    /// 根据模板代码和渠道获取模板
    /// </summary>
    /// <param name="code">模板代码</param>
    /// <param name="channel">消息渠道</param>
    /// <returns>模板详情</returns>
    [Authorize(policy: "permission:template.view")]
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code, [FromQuery] MessageChannel channel)
    {
        try
        {
            var result = await Grain.GetByCodeAsync(code, channel);
            
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

    /// <summary>
    /// 获取指定渠道的所有模板
    /// </summary>
    /// <param name="channel">消息渠道</param>
    /// <returns>模板列表</returns>
    [Authorize(policy: "permission:template.view")]
    [HttpGet("channel/{channel}")]
    public async Task<IActionResult> GetByChannelAsync(MessageChannel channel)
    {
        try
        {
            var results = await Grain.GetByChannelAsync(channel);
            return Ok(ResponseData.Success(data: results));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get templates by channel {Channel}", channel);
            return StatusCode(500, ResponseData.Fail(code: "get_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 获取分页模板列表
    /// </summary>
    /// <param name="channel">按消息渠道筛选</param>
    /// <param name="searchText">按名称或代码搜索</param>
    /// <param name="isActive">按激活状态筛选</param>
    /// <param name="page">页码（默认1）</param>
    /// <param name="pageSize">每页数量（默认20）</param>
    /// <returns>分页模板列表</returns>
    [Authorize(policy: "permission:template.view")]
    [HttpGet]
    public async Task<IActionResult> GetTemplatesAsync(
        [FromQuery] MessageChannel? channel,
        [FromQuery] string? searchText,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var (items, total) = await Grain.GetTemplatesAsync(channel, searchText, isActive, page, pageSize);
            return Ok(ResponseData.Success(data: new { Items = items, Total = total, Page = page, PageSize = pageSize }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get templates");
            return StatusCode(500, ResponseData.Fail(code: "get_failed", message: ex.Message));
        }
    }

    /// <summary>
    /// 预览模板渲染结果
    /// </summary>
    /// <param name="id">模板GUID</param>
    /// <param name="variables">模板变量</param>
    /// <returns>渲染后的预览内容</returns>
    [Authorize(policy: "permission:template.view")]
    [HttpPost("{id:guid}/preview")]
    public async Task<IActionResult> PreviewAsync(Guid id, [FromBody] Dictionary<string, string>? variables)
    {
        try
        {
            var result = await Grain.PreviewAsync(id, variables);
            return Ok(ResponseData.Success(data: result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to preview template {Id}", id);
            return StatusCode(500, ResponseData.Fail(code: "preview_failed", message: ex.Message));
        }
    }
}
