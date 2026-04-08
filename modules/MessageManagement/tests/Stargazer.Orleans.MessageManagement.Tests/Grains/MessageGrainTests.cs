using Microsoft.Extensions.Logging;
using Moq;
using Stargazer.Orleans.MessageManagement.Domain;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;
using Stargazer.Orleans.MessageManagement.Grains.Grains.Messages;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Email;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Push;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.Grains;

public class MessageGrainTests
{
    private readonly Mock<ILogger<MessageGrain>> _loggerMock;
    private readonly MessageSettings _settings;

    public MessageGrainTests()
    {
        _loggerMock = new Mock<ILogger<MessageGrain>>();
        _settings = new MessageSettings
        {
            Email = new EmailSettings { DefaultProvider = "smtp" },
            Sms = new SmsSettings { DefaultProvider = "aliyun" },
            Push = new PushSettings { DefaultProvider = "jpush" }
        };
    }

    [Fact]
    public void MessageSettings_ShouldHaveCorrectDefaults()
    {
        Assert.Equal("smtp", _settings.Email.DefaultProvider);
        Assert.Equal("aliyun", _settings.Sms.DefaultProvider);
        Assert.Equal("jpush", _settings.Push.DefaultProvider);
    }

    [Fact]
    public void SendMessageInputDto_ShouldHaveCorrectDefaults()
    {
        var input = new SendMessageInputDto
        {
            Channel = MessageChannel.Email,
            Receiver = "test@example.com",
            Content = "Test Content"
        };

        Assert.Equal(MessageChannel.Email, input.Channel);
        Assert.Equal("test@example.com", input.Receiver);
        Assert.Equal("Test Content", input.Content);
    }

    [Fact]
    public void MessageRecord_ShouldInitializeCorrectly()
    {
        var record = new MessageRecord
        {
            Id = Guid.NewGuid(),
            Channel = MessageChannel.Email,
            Receiver = "test@example.com",
            Content = "Test Content",
            Status = MessageStatus.Pending
        };

        Assert.Equal(MessageStatus.Pending, record.Status);
        Assert.Equal(MessageChannel.Email, record.Channel);
        Assert.Equal(0, record.RetryCount);
    }

    [Theory]
    [InlineData(MessageChannel.Email)]
    [InlineData(MessageChannel.Sms)]
    [InlineData(MessageChannel.Push)]
    public void MessageRecord_ShouldSupportAllChannels(MessageChannel channel)
    {
        var record = new MessageRecord { Channel = channel };
        Assert.Equal(channel, record.Channel);
    }

    [Theory]
    [InlineData(MessageStatus.Pending, 0)]
    [InlineData(MessageStatus.Sending, 1)]
    [InlineData(MessageStatus.Sent, 2)]
    [InlineData(MessageStatus.Delivered, 3)]
    [InlineData(MessageStatus.Failed, 4)]
    [InlineData(MessageStatus.Cancelled, 5)]
    public void MessageStatus_ShouldHaveCorrectValues(MessageStatus status, int expected)
    {
        Assert.Equal(expected, (int)status);
    }

    [Fact]
    public void EmailSendResult_ShouldSetProperties()
    {
        var result = new EmailSendResult
        {
            Success = true,
            MessageId = "msg-123"
        };

        Assert.True(result.Success);
        Assert.Equal("msg-123", result.MessageId);
    }

    [Fact]
    public void SmsSendResult_ShouldSetProperties()
    {
        var result = new SmsSendResult
        {
            Success = true,
            MessageId = "sms-123"
        };

        Assert.True(result.Success);
        Assert.Equal("sms-123", result.MessageId);
    }

    [Fact]
    public void PushSendResult_ShouldSetProperties()
    {
        var result = new PushSendResult
        {
            Success = true,
            MessageId = "push-123"
        };

        Assert.True(result.Success);
        Assert.Equal("push-123", result.MessageId);
    }
}
