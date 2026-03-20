namespace Stargazer.Orleans.MessageManagement.Domain;

/// <summary>
/// 消息模板实体
/// 用于定义可复用的消息模板
/// </summary>
public class MessageTemplate : Entity<Guid>
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
    /// 模板变量定义（JSON格式）
    /// </summary>
    public string Variables { get; set; } = "[]";

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 版本号
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// 默认Provider
    /// </summary>
    public string? DefaultProvider { get; set; }

    /// <summary>
    /// 标签（用于分组）
    /// </summary>
    public string? Tags { get; set; }
}
