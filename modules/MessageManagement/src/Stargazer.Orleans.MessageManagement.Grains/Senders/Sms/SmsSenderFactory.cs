using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;

namespace Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

public class SmsSenderFactory : ISmsSender
{
    private readonly SmsSettings _settings;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    public string ProviderName => "sms_factory";

    public SmsSenderFactory(IConfiguration configuration, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        _settings = configuration.GetSection("Message:Sms").Get<SmsSettings>() ?? new SmsSettings();
        _loggerFactory = loggerFactory;
        _httpClientFactory = httpClientFactory;
    }

    private ISmsSender GetProvider(string? providerName = null)
    {
        var name = providerName ?? _settings.DefaultProvider?.ToLower() ?? "aliyun";
        
        return name switch
        {
            "aliyun" when _settings.Aliyun != null => new AliyunSmsSender(_settings.Aliyun, _loggerFactory.CreateLogger<AliyunSmsSender>()),
            "tencent" when _settings.Tencent != null => new TencentSmsSender(_settings.Tencent, _loggerFactory.CreateLogger<TencentSmsSender>()),
            "huawei" when _settings.Huawei != null => new HuaweiSmsSender(_settings.Huawei, _loggerFactory.CreateLogger<HuaweiSmsSender>(), _httpClientFactory.CreateClient("HuaweiSms")),
            "ctyun" when _settings.Ctyun != null => new CtyunSmsSender(_settings.Ctyun, _loggerFactory.CreateLogger<CtyunSmsSender>(), _httpClientFactory.CreateClient("CtyunSms")),
            _ when _settings.Aliyun != null => new AliyunSmsSender(_settings.Aliyun, _loggerFactory.CreateLogger<AliyunSmsSender>()),
            _ => throw new NotSupportedException($"SMS provider '{name}' is not configured or not supported.")
        };
    }

    public async Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string templateCode,
        Dictionary<string, string>? templateParams = null,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        return await provider.SendAsync(phoneNumber, templateCode, templateParams, cancellationToken);
    }

    public async Task<List<SmsSendResult>> BatchSendAsync(
        List<SmsSendRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        return await provider.BatchSendAsync(requests, cancellationToken);
    }

    public async Task<SmsSendResult> SendTextAsync(
        string phoneNumber,
        string content,
        CancellationToken cancellationToken = default)
    {
        var provider = GetProvider();
        return await provider.SendTextAsync(phoneNumber, content, cancellationToken);
    }
}
