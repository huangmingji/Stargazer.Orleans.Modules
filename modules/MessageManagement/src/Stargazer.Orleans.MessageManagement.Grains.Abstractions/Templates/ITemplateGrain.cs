using Orleans;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates.Dtos;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates;

/// <summary>
/// 消息模板Grain接口
/// </summary>
public interface ITemplateGrain : IGrainWithIntegerKey
{
    /// <summary>
    /// 创建模板
    /// </summary>
    Task<TemplateDto> CreateAsync(CreateTemplateInputDto input);

    /// <summary>
    /// 更新模板
    /// </summary>
    Task<TemplateDto> UpdateAsync(UpdateTemplateInputDto input);

    /// <summary>
    /// 删除模板
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 获取模板
    /// </summary>
    Task<TemplateDto?> GetAsync(Guid id);

    /// <summary>
    /// 根据代码获取模板
    /// </summary>
    Task<TemplateDto?> GetByCodeAsync(string code, MessageChannel channel);

    /// <summary>
    /// 根据通道获取模板列表
    /// </summary>
    Task<List<TemplateDto>> GetByChannelAsync(MessageChannel channel);

    /// <summary>
    /// 预览模板渲染结果
    /// </summary>
    Task<string> PreviewAsync(Guid id, Dictionary<string, string>? variables);

    /// <summary>
    /// 获取模板列表
    /// </summary>
    Task<(List<TemplateDto> Items, int Total)> GetTemplatesAsync(
        MessageChannel? channel = null,
        string? searchText = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20);
}
