using Moq;
using Microsoft.Extensions.Logging;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.Senders;

public class SmsSenderTests
{
    private readonly Mock<ILogger<AliyunSmsSender>> _aliyunLoggerMock;
    private readonly Mock<ILogger<TencentSmsSender>> _tencentLoggerMock;
    private readonly Mock<ILogger<HuaweiSmsSender>> _huaweiLoggerMock;
    private readonly Mock<ILogger<CtyunSmsSender>> _ctyunLoggerMock;

    public SmsSenderTests()
    {
        _aliyunLoggerMock = new Mock<ILogger<AliyunSmsSender>>();
        _tencentLoggerMock = new Mock<ILogger<TencentSmsSender>>();
        _huaweiLoggerMock = new Mock<ILogger<HuaweiSmsSender>>();
        _ctyunLoggerMock = new Mock<ILogger<CtyunSmsSender>>();
    }

    [Fact]
    public void AliyunSmsSender_ShouldHaveCorrectProviderName()
    {
        var settings = new AliyunSmsSettings
        {
            AccessKeyId = "test-id",
            AccessKeySecret = "test-secret",
            SignName = "TestSign"
        };

        var sender = new AliyunSmsSender(settings, _aliyunLoggerMock.Object);
        
        Assert.Equal("aliyun", sender.ProviderName);
    }

    [Fact]
    public void TencentSmsSender_ShouldHaveCorrectProviderName()
    {
        var settings = new TencentSmsSettings
        {
            SecretId = "test-id",
            SecretKey = "test-secret",
            SdkAppId = "test-app",
            SmsSign = "TestSign"
        };

        var sender = new TencentSmsSender(settings, _tencentLoggerMock.Object);
        
        Assert.Equal("tencent", sender.ProviderName);
    }

    [Fact]
    public void HuaweiSmsSender_ShouldHaveCorrectProviderName()
    {
        var settings = new HuaweiSmsSettings
        {
            Ak = "test-ak",
            Sk = "test-sk",
            Sender = "12345"
        };
        
        using var httpClient = new HttpClient();
        var sender = new HuaweiSmsSender(settings, _huaweiLoggerMock.Object, httpClient);
        
        Assert.Equal("huawei", sender.ProviderName);
    }

    [Fact]
    public void CtyunSmsSender_ShouldHaveCorrectProviderName()
    {
        var settings = new CtyunSmsSettings
        {
            AccessKeyId = "test-id",
            AccessKeySecret = "test-secret",
            Signature = "TestSign",
            RequestUrl = "https://sms-global.ctapi.ctyun.cn/sms/api/v1"
        };
        
        using var httpClient = new HttpClient();
        var sender = new CtyunSmsSender(settings, _ctyunLoggerMock.Object, httpClient);
        
        Assert.Equal("ctyun", sender.ProviderName);
    }

    [Fact]
    public void AliyunSmsSender_ShouldThrowOnNullSettings()
    {
        Assert.Throws<ArgumentNullException>(() => 
            new AliyunSmsSender(null!, _aliyunLoggerMock.Object));
    }

    [Fact]
    public void HuaweiSmsSender_ShouldThrowOnNullSettings()
    {
        using var httpClient = new HttpClient();
        Assert.Throws<ArgumentNullException>(() => 
            new HuaweiSmsSender(null!, _huaweiLoggerMock.Object, httpClient));
    }

    [Fact]
    public void AliyunSmsSettings_ShouldHaveCorrectDefaults()
    {
        var settings = new AliyunSmsSettings();
        
        Assert.Equal("dysmsapi.aliyuncs.com", settings.Endpoint);
    }

    [Fact]
    public void TencentSmsSettings_ShouldHaveCorrectDefaults()
    {
        var settings = new TencentSmsSettings();
        
        Assert.Equal("ap-guangzhou", settings.Region);
    }

    [Fact]
    public void HuaweiSmsSettings_ShouldHaveCorrectDefaults()
    {
        var settings = new HuaweiSmsSettings();
        
        Assert.Equal("https://msgsms.cn-north-4.myhuaweicloud.com", settings.Endpoint);
    }

    [Fact]
    public void CtyunSmsSettings_ShouldHaveCorrectDefaults()
    {
        var settings = new CtyunSmsSettings();
        
        Assert.Equal("https://sms-global.ctapi.ctyun.cn/sms/api/v1", settings.RequestUrl);
    }

    public class PhoneNumberHelperTests
    {
        [Theory]
        [InlineData("+8613812345678", "+8613812345678")]
        [InlineData("8613812345678", "+8613812345678")]
        [InlineData("13812345678", "+8613812345678")]
        [InlineData("+11234567890", "+11234567890")]
        [InlineData("+44 20 7946 0958", "+44 20 7946 0958")]
        public void FormatForChina_ShouldNormalizePhoneNumber(string input, string expected)
        {
            var result = PhoneNumberHelper.FormatForChina(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData(null, "")]
        public void FormatForChina_ShouldHandleEmptyOrWhitespace(string? input, string expected)
        {
            var result = PhoneNumberHelper.FormatForChina(input!);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("13812345678", true)]
        [InlineData("+8613812345678", true)]
        [InlineData("8613812345678", true)]
        [InlineData("1234567", true)]
        [InlineData("123456789012345", true)]
        [InlineData("12345", false)]
        [InlineData("abc12345678", false)]
        [InlineData("", false)]
        [InlineData("+11234567890", false)]
        public void IsValidChinaPhoneNumber_ShouldValidateCorrectly(string input, bool expected)
        {
            var result = PhoneNumberHelper.IsValidChinaPhoneNumber(input);
            Assert.Equal(expected, result);
        }
    }
}
