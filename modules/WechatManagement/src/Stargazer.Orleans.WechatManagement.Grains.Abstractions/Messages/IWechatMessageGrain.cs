using Orleans;
using Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages.Dtos;

namespace Stargazer.Orleans.WechatManagement.Grains.Abstractions.Messages;

public interface IWechatMessageGrain : IGrainWithIntegerKey
{
    Task<WechatMessageLogDto?> GetMessageAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<WechatMessageLogDto> CreateMessageAsync(Guid id, CreateWechatMessageInputDto input, CancellationToken cancellationToken = default);
    
    Task<WechatMessageLogDto?> UpdateMessageStatusAsync(Guid id, int status, string? errorMessage = null, CancellationToken cancellationToken = default);
    
    Task<bool> CancelMessageAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IWechatMessageProducerGrain : IGrainWithStringKey
{
    Task<Guid> EnqueueTemplateMessageAsync(SendTemplateMessageInputDto input, CancellationToken cancellationToken = default);
    
    Task<Guid> EnqueueCustomMessageAsync(SendCustomMessageInputDto input, CancellationToken cancellationToken = default);
    
    Task<Guid> EnqueueMassMessageAsync(SendMassMessageInputDto input, CancellationToken cancellationToken = default);
    
    Task EnqueuePassiveReplyAsync(SendPassiveReplyInputDto input, CancellationToken cancellationToken = default);
}
