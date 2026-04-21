using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Streams;
using Stargazer.Common.SequentialGuid;
using Stargazer.Orleans.WechatManagement.Domain.Messages;
using Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Messages;

public class WechatMessageProducerGrain(IRepository<WechatMessageLog, Guid> repository)
    : Grain, IWechatMessageProducerGrain
{
    private IStreamProvider? _streamProvider;

    private static readonly string StreamNamespace = "WechatMessages";

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamProvider = this.GetStreamProvider("OrleansStreams");
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<Guid> EnqueueTemplateMessageAsync(SendTemplateMessageInputDto input, CancellationToken cancellationToken = default)
    {
        var messageId = await CreateMessageLogAsync(input.AccountId, input.OpenId, WechatMessageType.Template, input.TemplateId, null, cancellationToken);

        var streamMessage = new StreamMessage
        {
            MessageId = messageId,
            AccountId = input.AccountId,
            OpenId = input.OpenId,
            MessageType = WechatMessageType.Template,
            TemplateId = input.TemplateId,
            Data = JsonSerializer.Serialize(input.Data),
            Url = input.Url
        };

        var streamId = StreamId.Create(StreamNamespace, messageId);
        var stream = _streamProvider!.GetStream<StreamMessage>(streamId);
        await stream.OnNextAsync(streamMessage);

        return messageId;
    }

    public async Task<Guid> EnqueueCustomMessageAsync(SendCustomMessageInputDto input, CancellationToken cancellationToken = default)
    {
        var content = input.Content ?? input.Title ?? string.Empty;
        var messageId = await CreateMessageLogAsync(input.AccountId, input.OpenId, WechatMessageType.Custom, null, content, cancellationToken);

        var streamMessage = new StreamMessage
        {
            MessageId = messageId,
            AccountId = input.AccountId,
            OpenId = input.OpenId,
            MessageType = WechatMessageType.Custom,
            Data = JsonSerializer.Serialize(input)
        };

        var streamId = StreamId.Create(StreamNamespace, messageId);
        var stream = _streamProvider!.GetStream<StreamMessage>(streamId);
        await stream.OnNextAsync(streamMessage);

        return messageId;
    }

    public async Task<Guid> EnqueueMassMessageAsync(SendMassMessageInputDto input, CancellationToken cancellationToken = default)
    {
        var messageId = await CreateMessageLogAsync(input.AccountId, string.Join(",", input.OpenIds), WechatMessageType.Mass, input.MediaId, input.Content, cancellationToken);

        var streamMessage = new StreamMessage
        {
            MessageId = messageId,
            AccountId = input.AccountId,
            OpenId = string.Empty,
            MessageType = WechatMessageType.Mass,
            Data = JsonSerializer.Serialize(input)
        };

        var streamId = StreamId.Create(StreamNamespace, messageId);
        var stream = _streamProvider!.GetStream<StreamMessage>(streamId);
        await stream.OnNextAsync(streamMessage);

        return messageId;
    }

    public async Task EnqueuePassiveReplyAsync(SendPassiveReplyInputDto input, CancellationToken cancellationToken = default)
    {
        var messageId = await CreateMessageLogAsync(input.AccountId, input.OpenId, WechatMessageType.PassiveReply, null, input.Content, cancellationToken);

        var streamMessage = new StreamMessage
        {
            MessageId = messageId,
            AccountId = input.AccountId,
            OpenId = input.OpenId,
            MessageType = WechatMessageType.PassiveReply,
            Data = input.Content
        };

        var streamId = StreamId.Create(StreamNamespace, messageId);
        var stream = _streamProvider!.GetStream<StreamMessage>(streamId);
        await stream.OnNextAsync(streamMessage);
    }

    private async Task<Guid> CreateMessageLogAsync(Guid accountId, string openId, string messageType, string? templateId, string? content, CancellationToken cancellationToken)
    {
        var message = new WechatMessageLog
        {
            Id = new SequentialGuid().Create(),
            AccountId = accountId,
            OpenId = openId,
            MessageType = messageType,
            TemplateId = templateId,
            Content = content,
            Status = WechatMessageStatus.Pending,
            SendTime = DateTime.UtcNow,
            CreationTime = DateTime.UtcNow,
            LastModifyTime = DateTime.UtcNow
        };

        await repository.InsertAsync(message, cancellationToken);
        return message.Id;
    }
}

public class StreamMessage
{
    public Guid MessageId { get; set; }
    public Guid AccountId { get; set; }
    public string OpenId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public string? Data { get; set; }
    public string? Url { get; set; }
}
