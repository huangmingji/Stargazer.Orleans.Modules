using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;
using Xunit;

namespace Stargazer.Orleans.MessageManagement.Tests.Integration;

public class MessageControllerIntegrationTests : IntegrationTestBase
{
    public MessageControllerIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Send_WithEmptyReceiver_ReturnsBadRequest()
    {
        var input = new SendMessageInputDto
        {
            Channel = MessageChannel.Email,
            Receiver = "",
            Content = "Test content"
        };

        var (success, data, errorCode) = await PostAsync<MessageRecordDto>("api/message/send", input);

        Assert.False(success);
        Assert.Equal("invalid_receiver", errorCode);
    }

    [Fact]
    public async Task Send_WithEmptyContentAndNoTemplate_ReturnsBadRequest()
    {
        var input = new SendMessageInputDto
        {
            Channel = MessageChannel.Email,
            Receiver = "test@example.com",
            Content = ""
        };

        var (success, data, errorCode) = await PostAsync<MessageRecordDto>("api/message/send", input);

        Assert.False(success);
        Assert.Equal("invalid_content", errorCode);
    }

    [Fact]
    public async Task Send_WithValidEmail_ReturnsSuccess()
    {
        var input = new SendMessageInputDto
        {
            Channel = MessageChannel.Email,
            Receiver = $"test_{Guid.NewGuid():N}@example.com",
            Subject = "Test Subject",
            Content = "Test content"
        };

        var (success, data, errorCode) = await PostAsync<MessageRecordDto>("api/message/send", input);

        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal(MessageChannel.Email.ToString(), data.Channel);
    }

    [Fact]
    public async Task Send_WithValidSms_ReturnsSuccess()
    {
        var input = new SendMessageInputDto
        {
            Channel = MessageChannel.Sms,
            Receiver = "13800138000",
            Content = "Test SMS content"
        };

        var (success, data, errorCode) = await PostAsync<MessageRecordDto>("api/message/send", input);

        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal(MessageChannel.Sms.ToString(), data.Channel);
    }

    [Fact]
    public async Task Send_WithValidPush_ReturnsSuccess()
    {
        var input = new SendMessageInputDto
        {
            Channel = MessageChannel.Push,
            Receiver = $"device_token_{Guid.NewGuid():N}",
            Subject = "Push Title",
            Content = "Push content"
        };

        var (success, data, errorCode) = await PostAsync<MessageRecordDto>("api/message/send", input);

        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal(MessageChannel.Push.ToString(), data.Channel);
    }

    [Fact]
    public async Task Send_WithTemplateCode_ReturnsSuccess()
    {
        var input = new SendMessageInputDto
        {
            Channel = MessageChannel.Email,
            Receiver = "test@example.com",
            TemplateCode = "test_template",
            Content = ""
        };

        var (success, data, errorCode) = await PostAsync<MessageRecordDto>("api/message/send", input);

        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal("test_template", data.TemplateCode);
    }

    [Fact]
    public async Task BatchSend_WithEmptyReceivers_ReturnsBadRequest()
    {
        var input = new BatchSendMessageInputDto
        {
            Channel = MessageChannel.Email,
            Receivers = new List<string>(),
            Content = "Test content"
        };

        var (success, data, errorCode) = await PostAsync<List<MessageRecordDto>>("api/message/batch-send", input);

        Assert.False(success);
        Assert.Equal("invalid_receivers", errorCode);
    }

    [Fact]
    public async Task BatchSend_WithValidReceivers_ReturnsSuccess()
    {
        var input = new BatchSendMessageInputDto
        {
            Channel = MessageChannel.Email,
            Receivers = new List<string>
            {
                $"user1_{Guid.NewGuid():N}@example.com",
                $"user2_{Guid.NewGuid():N}@example.com",
                $"user3_{Guid.NewGuid():N}@example.com"
            },
            Subject = "Batch Test",
            Content = "Batch test content"
        };

        var (success, data, errorCode) = await PostAsync<List<MessageRecordDto>>("api/message/batch-send", input);

        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal(3, data.Count);
    }

    [Fact]
    public async Task GetRecord_WithNonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();

        var (success, data, errorCode) = await GetAsync<MessageRecordDto>($"api/message/{nonExistentId}");

        Assert.False(success);
        Assert.Equal("record_not_found", errorCode);
    }

    [Fact]
    public async Task GetRecords_WithNoFilters_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<PagedResult<MessageRecordDto>>("api/message");

        Assert.True(success);
        Assert.NotNull(data);
    }

    [Fact]
    public async Task GetRecords_WithChannelFilter_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<PagedResult<MessageRecordDto>>("api/message?channel=Email");

        Assert.True(success);
        Assert.NotNull(data);
    }

    [Fact]
    public async Task GetRecords_WithStatusFilter_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<PagedResult<MessageRecordDto>>("api/message?status=Pending");

        Assert.True(success);
        Assert.NotNull(data);
    }

    [Fact]
    public async Task GetRecords_WithPagination_ReturnsSuccess()
    {
        var (success, data, errorCode) = await GetAsync<PagedResult<MessageRecordDto>>("api/message?page=1&pageSize=10");

        Assert.True(success);
        Assert.NotNull(data);
        Assert.Equal(1, data.Page);
        Assert.Equal(10, data.PageSize);
    }

    [Fact]
    public async Task Cancel_WithNonExistentId_ReturnsBadRequest()
    {
        var nonExistentId = Guid.NewGuid();

        var (success, data, errorCode) = await PostAsync<bool>($"api/message/{nonExistentId}/cancel");

        Assert.False(success);
        Assert.Equal("cancel_failed", errorCode);
    }

    [Fact]
    public async Task Retry_WithNonExistentId_ReturnsError()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync($"api/message/{nonExistentId}/retry", null);
        
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                    response.StatusCode == System.Net.HttpStatusCode.BadRequest);
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
