using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Authorization;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates.Dtos;

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
    [Authorize(policy: $"permission:{AuthorizationPermissions.Templates.Create}")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TemplateDto))]
    public async Task<TemplateDto> CreateAsync([FromBody] CreateTemplateInputDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new ArgumentException("invalid_name");
        }

        if (string.IsNullOrWhiteSpace(input.Code))
        {
            throw new ArgumentException("invalid_code");
        }

        if (string.IsNullOrWhiteSpace(input.ContentTemplate))
        {
            throw new ArgumentException("invalid_content");
        }

        return await Grain.CreateAsync(input);
    }

    /// <summary>
    /// 更新现有模板
    /// </summary>
    /// <param name="input">模板更新输入</param>
    /// <returns>更新后的模板详情</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Templates.Update}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TemplateDto))]
    public async Task<TemplateDto> UpdateAsync([FromBody] UpdateTemplateInputDto input)
    {
        if (input.Id == Guid.Empty)
        {
            throw new ArgumentException("invalid_id");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new ArgumentException("invalid_name");
        }

        return await Grain.UpdateAsync(input);
    }

    /// <summary>
    /// 根据ID删除模板
    /// </summary>
    /// <param name="id">模板GUID</param>
    /// <returns>删除结果</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Templates.Delete}")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task DeleteAsync(Guid id)
    {
        if (await Grain.GetAsync(id) == null)
        {
            throw new KeyNotFoundException("template_not_found");
        }

        await Grain.DeleteAsync(id);
    }

    /// <summary>
    /// 根据ID获取模板
    /// </summary>
    /// <param name="id">模板GUID</param>
    /// <returns>模板详情</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Templates.View}")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TemplateDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<TemplateDto> GetAsync(Guid id)
    {
        var result = await Grain.GetAsync(id);

        if (result == null)
        {
            throw new KeyNotFoundException("template_not_found");
        }

        return result;
    }

    /// <summary>
    /// 根据模板代码和渠道获取模板
    /// </summary>
    /// <param name="code">模板代码</param>
    /// <param name="channel">消息渠道</param>
    /// <returns>模板详情</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Templates.View}")]
    [HttpGet("code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TemplateDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<TemplateDto> GetByCodeAsync(string code, [FromQuery] MessageChannel channel)
    {
        var result = await Grain.GetByCodeAsync(code, channel);

        if (result == null)
        {
            throw new KeyNotFoundException("template_not_found");
        }

        return result;
    }

    /// <summary>
    /// 获取指定渠道的所有模板
    /// </summary>
    /// <param name="channel">消息渠道</param>
    /// <returns>模板列表</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Templates.View}")]
    [HttpGet("channel/{channel}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TemplateDto>))]
    public async Task<List<TemplateDto>> GetByChannelAsync(MessageChannel channel)
    {
        return await Grain.GetByChannelAsync(channel);
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
    [Authorize(policy: $"permission:{AuthorizationPermissions.Templates.View}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PageResult<TemplateDto>))]
    public async Task<PageResult<TemplateDto>> GetTemplatesAsync(
        [FromQuery] MessageChannel? channel,
        [FromQuery] string? searchText,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await Grain.GetTemplatesAsync(channel, searchText, isActive, page, pageSize);
    }

    /// <summary>
    /// 预览模板渲染结果
    /// </summary>
    /// <param name="id">模板GUID</param>
    /// <param name="variables">模板变量</param>
    /// <returns>渲染后的预览内容</returns>
    [Authorize(policy: $"permission:{AuthorizationPermissions.Templates.View}")]
    [HttpPost("{id:guid}/preview")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseData))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseData))]
    public async Task<ResponseData> PreviewAsync(Guid id, [FromBody] Dictionary<string, string>? variables)
    {
        var result = await Grain.PreviewAsync(id, variables);
        return ResponseData.Success(data: result);
    }
}
