using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Push;

/// <summary>
/// JPush (极光推送) 发送器实现
/// </summary>
public class JPushSender : IPushSender
{
    private readonly JPushSettings _settings;
    private readonly ILogger<JPushSender> _logger;

    /// <inheritdoc />
    public string ProviderName => "jpush";

    /// <summary>
    /// 初始化 JPushSender 实例
    /// </summary>
    /// <param name="settings">JPush 配置</param>
    /// <param name="logger">日志记录器</param>
    public JPushSender(JPushSettings settings, ILogger<JPushSender> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<PushSendResult> SendAsync(
        PushRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("JPush SendAsync is not implemented yet");
        throw new NotImplementedException("JPush sending functionality is not yet implemented");
    }

    /// <inheritdoc />
    public Task<List<PushSendResult>> BatchSendAsync(
        List<PushRequest> requests,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("JPush BatchSendAsync is not implemented yet");
        throw new NotImplementedException("JPush batch sending functionality is not yet implemented");
    }
}
