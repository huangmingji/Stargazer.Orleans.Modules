using Stargazer.Orleans.MessageManagement.Domain.Shared;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Templates.Dtos;

/// <summary>
/// 创建模板输入参数
/// </summary>
public class CreateTemplateInputDto
{
    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板代码（唯一）
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 所属通道
    /// </summary>
    public MessageChannel Channel { get; set; }

    /// <summary>
    /// 邮件主题模板（Email专用）
    /// </summary>
    public string? SubjectTemplate { get; set; }

    /// <summary>
    /// 内容模板，支持 {{variable}} 占位符
    /// </summary>
    public string ContentTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 变量定义
    /// </summary>
    public List<TemplateVariableDto>? Variables { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 默认Provider
    /// </summary>
    public string? DefaultProvider { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string? Tags { get; set; }
}

/// <summary>
/// 更新模板输入参数
/// </summary>
public class UpdateTemplateInputDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? SubjectTemplate { get; set; }

    public string ContentTemplate { get; set; } = string.Empty;

    public List<TemplateVariableDto>? Variables { get; set; }

    public string? Description { get; set; }

    public string? DefaultProvider { get; set; }

    public string? Tags { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 模板变量定义
/// </summary>
public class TemplateVariableDto
{
    /// <summary>
    /// 变量名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 变量类型
    /// </summary>
    public string Type { get; set; } = "string";

    /// <summary>
    /// 是否必填
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public string? DefaultValue { get; set; }
}

/// <summary>
/// 模板输出DTO
/// </summary>
public class TemplateDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Channel { get; set; } = string.Empty;

    public string? SubjectTemplate { get; set; }

    public string ContentTemplate { get; set; } = string.Empty;

    public List<TemplateVariableDto>? Variables { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int Version { get; set; }

    public string? DefaultProvider { get; set; }

    public string? Tags { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime LastModifyTime { get; set; }
}
