using Stargazer.Orleans.MessageManagement.Domain.Shared;

namespace Stargazer.Orleans.MessageManagement.Domain;

/// <summary>
/// Provider配置实体
/// 存储各消息Provider的连接和认证配置
/// </summary>
public class ProviderConfig : Entity<Guid>
{
    /// <summary>
    /// Provider名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 所属通道
    /// </summary>
    public MessageChannel Channel { get; set; }

    /// <summary>
    /// 配置JSON（加密存储敏感信息）
    /// </summary>
    public string ConfigJson { get; set; } = "{}";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 优先级（数字越小优先级越高）
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 最大QPS
    /// </summary>
    public int MaxQps { get; set; } = 100;

    /// <summary>
    /// 当前QPS
    /// </summary>
    public int CurrentQps { get; set; }

    /// <summary>
    /// 健康状态
    /// </summary>
    public bool IsHealthy { get; set; } = true;

    /// <summary>
    /// 最后检查时间
    /// </summary>
    public DateTime? LastCheckAt { get; set; }
}
