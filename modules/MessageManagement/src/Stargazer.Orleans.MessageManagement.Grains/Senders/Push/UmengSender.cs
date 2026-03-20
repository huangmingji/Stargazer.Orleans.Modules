using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Push;

/// <summary>
/// Umeng (友盟推送) 发送器实现
/// </summary>
public class UmengSender(UmengSettings settings, ILogger<UmengSender> logger) : IPushSender
{
    private readonly UmengSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly ILogger<UmengSender> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public string ProviderName => "umeng";

    // /// <summary>
    // /// 初始化 UmengSender 实例
    // /// </summary>
    // /// <param name="settings">Umeng 配置</param>
    // /// <param name="logger">日志记录器</param>
    // public UmengSender(UmengSettings settings, ILogger<UmengSender> logger)
    // {
    //     _settings = settings;
    //     _logger = logger;
    // }

    /// <inheritdoc />
    public Task<PushSendResult> SendAsync(
        PushRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Umeng SendAsync is not implemented yet");
        throw new NotImplementedException("Umeng sending functionality is not yet implemented");
    }

    /// <inheritdoc />
    public Task<List<PushSendResult>> BatchSendAsync(
        List<PushRequest> requests,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Umeng BatchSendAsync is not implemented yet");
        throw new NotImplementedException("Umeng batch sending functionality is not yet implemented");
    }
}
