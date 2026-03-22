using Orleans;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages;

/// <summary>
/// 消息发送Grain接口
/// </summary>
public interface IMessageGrain : IGrainWithIntegerKey
{
    /// <summary>
    /// 发送消息
    /// </summary>
    Task<MessageRecordDto> SendAsync(SendMessageInputDto input);

    /// <summary>
    /// 批量发送消息
    /// </summary>
    Task<List<MessageRecordDto>> BatchSendAsync(BatchSendMessageInputDto input);

    /// <summary>
    /// 获取消息记录
    /// </summary>
    Task<MessageRecordDto?> GetRecordAsync(Guid id);

    /// <summary>
    /// 获取消息记录列表
    /// </summary>
    Task<(List<MessageRecordDto> Items, int Total)> GetRecordsAsync(
        string? channel = null,
        string? status = null,
        string? receiver = null,
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// 重试发送失败的消息
    /// </summary>
    Task<MessageRecordDto> RetryAsync(Guid id);

    /// <summary>
    /// 取消定时发送的消息
    /// </summary>
    Task<bool> CancelAsync(Guid id);
}
