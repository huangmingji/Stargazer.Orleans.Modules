using System.Text.Json.Serialization;
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
    [Id(0)] [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板代码（唯一）
    /// </summary>
    [Id(1)] [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 所属通道
    /// </summary>
    [Id(2)] [JsonPropertyName("channel")]
    public MessageChannel Channel { get; set; }

    /// <summary>
    /// 邮件主题模板（Email专用）
    /// </summary>
    [Id(3)] [JsonPropertyName("subject_template")]
    public string? SubjectTemplate { get; set; }

    /// <summary>
    /// 内容模板，支持 {{variable}} 占位符
    /// </summary>
    [Id(4)] [JsonPropertyName("content_template")]
    public string ContentTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 变量定义
    /// </summary>
    [Id(5)] [JsonPropertyName("variables")]
    public List<TemplateVariableDto>? Variables { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [Id(6)] [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 默认Provider
    /// </summary>
    [Id(7)] [JsonPropertyName("default_provider")]
    public string? DefaultProvider { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [Id(8)] [JsonPropertyName("tags")]
    public string? Tags { get; set; }
}

/// <summary>
/// 更新模板输入参数
/// </summary>
[GenerateSerializer]
public class UpdateTemplateInputDto
{
    [Id(0)] [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Id(1)] [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Id(2)] [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [Id(3)] [JsonPropertyName("subject_template")]
    public string? SubjectTemplate { get; set; }

    [Id(4)] [JsonPropertyName("content_template")]
    public string ContentTemplate { get; set; } = string.Empty;

    [Id(5)] [JsonPropertyName("variables")]
    public List<TemplateVariableDto>? Variables { get; set; }

    [Id(6)] [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Id(7)] [JsonPropertyName("default_provider")]
    public string? DefaultProvider { get; set; }

    [Id(8)] [JsonPropertyName("tags")]
    public string? Tags { get; set; }

    [Id(9)] [JsonPropertyName("is_active")]
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
    [Id(0)] [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 变量类型
    /// </summary>
    [Id(1)] [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    /// <summary>
    /// 是否必填
    /// </summary>
    [Id(2)] [JsonPropertyName("required")]
    public bool Required { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    [Id(3)] [JsonPropertyName("default_value")]
    public string? DefaultValue { get; set; }
}

/// <summary>
/// 模板输出DTO
/// </summary>
[GenerateSerializer]
public class TemplateDto
{
    [Id(0)] [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [Id(1)] [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Id(2)] [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [Id(3)] [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [Id(4)] [JsonPropertyName("subject_template")]
    public string? SubjectTemplate { get; set; }

    [Id(5)] [JsonPropertyName("content_template")]
    public string ContentTemplate { get; set; } = string.Empty;

    [Id(6)] [JsonPropertyName("variables")]
    public List<TemplateVariableDto>? Variables { get; set; }

    [Id(7)] [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Id(8)] [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [Id(9)] [JsonPropertyName("version")]
    public int Version { get; set; }

    [Id(10)] [JsonPropertyName("default_provider")]
    public string? DefaultProvider { get; set; }

    [Id(11)] [JsonPropertyName("tags")]
    public string? Tags { get; set; }

    [Id(12)] [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }

    [Id(13)] [JsonPropertyName("last_modify_time")]
    public DateTime LastModifyTime { get; set; }
}
