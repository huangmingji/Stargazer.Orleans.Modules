using Stargazer.Orleans.MessageManagement.Domain;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.Domain;

public class EntityTests
{
    [Fact]
    public void MessageRecord_ShouldInitializeWithCorrectDefaults()
    {
        var record = new MessageRecord();
        
        Assert.Equal(MessageStatus.Pending, record.Status);
        Assert.Equal(0, record.RetryCount);
    }

    [Fact]
    public void MessageRecord_ShouldSetChannelCorrectly()
    {
        var record = new MessageRecord
        {
            Channel = MessageChannel.Email
        };
        
        Assert.Equal(MessageChannel.Email, record.Channel);
    }

    [Fact]
    public void MessageTemplate_ShouldInitializeCorrectly()
    {
        var template = new MessageTemplate
        {
            Name = "Test Template",
            Code = "test_code",
            Channel = MessageChannel.Sms
        };
        
        Assert.Equal("Test Template", template.Name);
        Assert.Equal("test_code", template.Code);
        Assert.Equal(MessageChannel.Sms, template.Channel);
        Assert.True(template.IsActive);
    }

    [Fact]
    public void ProviderConfig_ShouldSetPropertiesCorrectly()
    {
        var config = new ProviderConfig
        {
            Name = "aliyun",
            Channel = MessageChannel.Sms,
            IsEnabled = true,
            Priority = 1,
            MaxQps = 100
        };
        
        Assert.Equal("aliyun", config.Name);
        Assert.True(config.IsEnabled);
        Assert.Equal(100, config.MaxQps);
    }
}

public class MessageChannelTests
{
    [Theory]
    [InlineData(MessageChannel.Email, 1)]
    [InlineData(MessageChannel.Sms, 2)]
    [InlineData(MessageChannel.Push, 3)]
    public void MessageChannel_ShouldHaveCorrectValues(MessageChannel channel, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)channel);
    }
}

public class MessageStatusTests
{
    [Theory]
    [InlineData(MessageStatus.Pending, 0)]
    [InlineData(MessageStatus.Sending, 1)]
    [InlineData(MessageStatus.Sent, 2)]
    [InlineData(MessageStatus.Delivered, 3)]
    [InlineData(MessageStatus.Failed, 4)]
    [InlineData(MessageStatus.Cancelled, 5)]
    public void MessageStatus_ShouldHaveCorrectValues(MessageStatus status, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)status);
    }
}
