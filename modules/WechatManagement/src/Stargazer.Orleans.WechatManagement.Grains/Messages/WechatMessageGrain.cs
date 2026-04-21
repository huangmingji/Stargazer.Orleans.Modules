using Orleans;
using Orleans.Concurrency;
using Stargazer.Orleans.WechatManagement.Domain.Messages;
using Stargazer.Orleans.WechatManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Messages;

[StatelessWorker]
public class WechatMessageGrain(IRepository<WechatMessageLog, Guid> repository) : Grain, IWechatMessageGrain
{
    public async Task<WechatMessageLogDto?> GetMessageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await repository.FindAsync(id, cancellationToken);
        return message == null ? null : MapToDto(message);
    }

    public async Task<WechatMessageLogDto> CreateMessageAsync(Guid id, CreateWechatMessageInputDto input, CancellationToken cancellationToken = default)
    {
        var message = new WechatMessageLog
        {
            Id = this.GetPrimaryKey(),
            AccountId = input.AccountId,
            OpenId = input.OpenId,
            MessageType = input.MessageType,
            TemplateId = input.TemplateId,
            Content = input.Content,
            Status = WechatMessageStatus.Pending,
            SendTime = DateTime.UtcNow,
            CreationTime = DateTime.UtcNow,
            LastModifyTime = DateTime.UtcNow
        };

        await repository.InsertAsync(message, cancellationToken);
        return MapToDto(message);
    }

    public async Task<WechatMessageLogDto?> UpdateMessageStatusAsync(Guid id, int status, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        var message = await repository.FindAsync(id, cancellationToken);
        if (message == null) return null;

        message.Status = status;
        if (errorMessage != null) message.ErrorMessage = errorMessage;
        if (status == WechatMessageStatus.Success || status == WechatMessageStatus.Failed)
        {
            message.CompleteTime = DateTime.UtcNow;
        }
        message.LastModifyTime = DateTime.UtcNow;

        await repository.UpdateAsync(message, cancellationToken);
        return MapToDto(message);
    }

    public async Task<bool> CancelMessageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await UpdateMessageStatusAsync(id, WechatMessageStatus.Cancelled, cancellationToken: cancellationToken);
        return result != null;
    }

    private static WechatMessageLogDto MapToDto(WechatMessageLog message)
    {
        return new WechatMessageLogDto
        {
            Id = message.Id,
            AccountId = message.AccountId,
            OpenId = message.OpenId,
            MessageType = message.MessageType,
            TemplateId = message.TemplateId,
            Content = message.Content,
            Status = message.Status,
            ErrorMessage = message.ErrorMessage,
            SendTime = message.SendTime,
            CompleteTime = message.CompleteTime,
            MsgId = message.MsgId,
            CreationTime = message.CreationTime
        };
    }
}