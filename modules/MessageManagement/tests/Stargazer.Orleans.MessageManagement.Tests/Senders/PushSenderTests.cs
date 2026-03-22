using Moq;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Push;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.Senders;

public class PushSenderTests
{
    [Fact]
    public void JPushSender_ShouldHaveCorrectProviderName()
    {
        var settings = new JPushSettings
        {
            AppKey = "test-app-key",
            MasterSecret = "test-master-secret"
        };
        
        using var httpClient = new HttpClient();
        var loggerMock = new Mock<ILogger<JPushSender>>();
        var sender = new JPushSender(settings, loggerMock.Object, httpClient);
        
        Assert.Equal("jpush", sender.ProviderName);
    }

    [Fact]
    public void UmengSender_ShouldHaveCorrectProviderName()
    {
        var settings = new UmengSettings
        {
            AppKey = "test-app-key",
            AppMasterSecret = "test-master-secret"
        };
        
        using var httpClient = new HttpClient();
        var loggerMock = new Mock<ILogger<UmengSender>>();
        var sender = new UmengSender(settings, loggerMock.Object, httpClient);
        
        Assert.Equal("umeng", sender.ProviderName);
    }

    [Fact]
    public void JPushSender_ShouldCreateInstance()
    {
        var settings = new JPushSettings
        {
            AppKey = "test-key",
            MasterSecret = "test-secret"
        };
        using var httpClient = new HttpClient();
        var loggerMock = new Mock<ILogger<JPushSender>>();
        var sender = new JPushSender(settings, loggerMock.Object, httpClient);
        
        Assert.NotNull(sender);
        Assert.Equal("jpush", sender.ProviderName);
    }

    [Fact]
    public void UmengSender_ShouldCreateInstance()
    {
        var settings = new UmengSettings
        {
            AppKey = "test-key",
            AppMasterSecret = "test-secret"
        };
        using var httpClient = new HttpClient();
        var loggerMock = new Mock<ILogger<UmengSender>>();
        var sender = new UmengSender(settings, loggerMock.Object, httpClient);
        
        Assert.NotNull(sender);
        Assert.Equal("umeng", sender.ProviderName);
    }

    [Fact]
    public void PushRequest_ShouldHaveCorrectDefaults()
    {
        var request = new PushRequest();
        
        Assert.Equal("all", request.TargetType);
        Assert.True(request.ApnsProduction);
        Assert.NotNull(request.Targets);
        Assert.Empty(request.Targets);
    }

    [Fact]
    public void PushSendResult_ShouldSetPropertiesCorrectly()
    {
        var result = new PushSendResult
        {
            Success = true,
            MessageId = "msg-123",
            ErrorMessage = null
        };
        
        Assert.True(result.Success);
        Assert.Equal("msg-123", result.MessageId);
    }

    [Fact]
    public void JPushSettings_ShouldHaveCorrectDefaults()
    {
        var settings = new JPushSettings();
        
        Assert.Equal(string.Empty, settings.AppKey);
        Assert.Equal(string.Empty, settings.MasterSecret);
    }

    [Fact]
    public void UmengSettings_ShouldHaveCorrectDefaults()
    {
        var settings = new UmengSettings();
        
        Assert.Equal(string.Empty, settings.AppKey);
        Assert.Equal(string.Empty, settings.AppMasterSecret);
    }

    [Fact]
    public void PushSettings_ShouldHaveCorrectDefaults()
    {
        var settings = new PushSettings();
        
        Assert.Equal("jpush", settings.DefaultProvider);
    }
}
