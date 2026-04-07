using Stargazer.Orleans.MessageManagement.Domain.Shared;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates.Dtos;

/// <summary>
/// 创建模板输入参数
/// </summary>
[GenerateSerializer]
public class CreateTemplateInputDto
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [Id(0)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板代码（唯一）
    /// </summary>
    [Id(1)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 所属通道
    /// </summary>
    [Id(2)]
    public MessageChannel Channel { get; set; }

    /// <summary>
    /// 邮件主题模板（Email专用）
    /// </summary>
    [Id(3)]
    public string? SubjectTemplate { get; set; }

    /// <summary>
    /// 内容模板，支持 {{variable}} 占位符
    /// </summary>
    [Id(4)]
    public string ContentTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 变量定义
    /// </summary>
    [Id(5)]
    public List<TemplateVariableDto>? Variables { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [Id(6)]
    public string? Description { get; set; }

    /// <summary>
    /// 默认Provider
    /// </summary>
    [Id(7)]
    public string? DefaultProvider { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [Id(8)]
    public string? Tags { get; set; }
}

/// <summary>
/// 更新模板输入参数
/// </summary>
[GenerateSerializer]
public class UpdateTemplateInputDto
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public string Name { get; set; } = string.Empty;

    [Id(2)]
    public string Code { get; set; } = string.Empty;

    [Id(3)]
    public string? SubjectTemplate { get; set; }

    [Id(4)]
    public string ContentTemplate { get; set; } = string.Empty;

    [Id(5)]
    public List<TemplateVariableDto>? Variables { get; set; }

    [Id(6)]
    public string? Description { get; set; }

    [Id(7)]
    public string? DefaultProvider { get; set; }

    [Id(8)]
    public string? Tags { get; set; }

    [Id(9)]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 模板变量定义
/// </summary>
[GenerateSerializer]
public class TemplateVariableDto
{
    /// <summary>
    /// 变量名称
    /// </summary>
    [Id(0)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 变量类型
    /// </summary>
    [Id(1)]
    public string Type { get; set; } = "string";

    /// <summary>
    /// 是否必填
    /// </summary>
    [Id(2)]
    public bool Required { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    [Id(3)]
    public string? DefaultValue { get; set; }
}

/// <summary>
/// 模板输出DTO
/// </summary>
[GenerateSerializer]
public class TemplateDto
{
    [Id(0)]
    public Guid Id { get; set; }

    [Id(1)]
    public string Name { get; set; } = string.Empty;

    [Id(2)]
    public string Code { get; set; } = string.Empty;

    [Id(3)]
    public string Channel { get; set; } = string.Empty;

    [Id(4)]
    public string? SubjectTemplate { get; set; }

    [Id(5)]
    public string ContentTemplate { get; set; } = string.Empty;

    [Id(6)]
    public List<TemplateVariableDto>? Variables { get; set; }

    [Id(7)]
    public string? Description { get; set; }

    [Id(8)]
    public bool IsActive { get; set; }

    [Id(9)]
    public int Version { get; set; }

    [Id(10)]
    public string? DefaultProvider { get; set; }

    [Id(11)]
    public string? Tags { get; set; }

    [Id(12)]
    public DateTime CreationTime { get; set; }

    [Id(13)]
    public DateTime LastModifyTime { get; set; }
}
