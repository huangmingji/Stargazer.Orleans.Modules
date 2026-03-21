using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Stargazer.Orleans.MessageManagement.Domain;
using Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages.Dtos;
using Stargazer.Orleans.MessageManagement.Grains.Configuration;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Email;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Push;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

namespace Stargazer.Orleans.MessageManagement.Grains.Grains.Messages;

/// <summary>
/// 消息发送 Grain 实现
/// </summary>
[StatelessWorker]
public class MessageGrain : Grain, IMessageGrain
{
    private readonly IRepository<MessageRecord, Guid> _recordRepository;
    private readonly IRepository<MessageTemplate, Guid> _templateRepository;
    private readonly IEnumerable<IEmailSender> _emailSenders;
    private readonly IEnumerable<ISmsSender> _smsSenders;
    private readonly IEnumerable<IPushSender> _pushSenders;
    private readonly MessageSettings _settings;
    private readonly ILogger<MessageGrain> _logger;

    public MessageGrain(
        IRepository<MessageRecord, Guid> recordRepository,
        IRepository<MessageTemplate, Guid> templateRepository,
        IEnumerable<IEmailSender> emailSenders,
        IEnumerable<ISmsSender> smsSenders,
        IEnumerable<IPushSender> pushSenders,
        MessageSettings settings,
        ILogger<MessageGrain> logger)
    {
        _recordRepository = recordRepository;
        _templateRepository = templateRepository;
        _emailSenders = emailSenders;
        _smsSenders = smsSenders;
        _pushSenders = pushSenders;
        _settings = settings;
        _logger = logger;
    }

    public async Task<MessageRecordDto> SendAsync(SendMessageInputDto input)
    {
        var channel = (MessageChannel)input.Channel;

        var record = new MessageRecord
        {
            Id = Guid.NewGuid(),
            Channel = channel,
            TemplateId = null,
            TemplateCode = input.TemplateCode,
            Receiver = input.Receiver,
            Subject = input.Subject,
            Content = input.Content,
            Variables = input.Variables != null
                ? JsonSerializer.Serialize(input.Variables)
                : null,
            Provider = input.Provider ?? GetDefaultProvider(channel),
            Status = MessageStatus.Pending,
            ScheduledAt = input.ScheduledAt,
            SenderId = input.SenderId,
            BusinessId = input.BusinessId,
            BusinessType = input.BusinessType,
            CreatorId = Guid.Empty,
            CreationTime = DateTime.UtcNow
        };

        await _recordRepository.InsertAsync(record);

        if (input.ScheduledAt.HasValue && input.ScheduledAt > DateTime.UtcNow)
        {
            _logger.LogInformation("Message {RecordId} scheduled for {ScheduledAt}", record.Id, input.ScheduledAt);
            return ToDto(record);
        }

        await SendMessageInternal(record);

        return ToDto(record);
    }

    public async Task<List<MessageRecordDto>> BatchSendAsync(BatchSendMessageInputDto input)
    {
        var channel = (MessageChannel)input.Channel;
        var records = new List<MessageRecord>();

        foreach (var receiver in input.Receivers)
        {
            var record = new MessageRecord
            {
                Id = Guid.NewGuid(),
                Channel = channel,
                TemplateCode = input.TemplateCode,
                Receiver = receiver,
                Subject = input.Subject,
                Content = input.Content,
                Variables = input.Variables != null
                    ? JsonSerializer.Serialize(input.Variables)
                    : null,
                Provider = input.Provider ?? GetDefaultProvider(channel),
                Status = MessageStatus.Pending,
                SenderId = input.SenderId,
                BusinessId = input.BusinessId,
                BusinessType = input.BusinessType,
                CreatorId = Guid.Empty,
                CreationTime = DateTime.UtcNow
            };
            records.Add(record);
        }

        try
        {
            await _recordRepository.BeginTransactionAsync();

            await _recordRepository.InsertAsync(records);

            var tasks = records.Select(SendMessageInternal);
            await Task.WhenAll(tasks);

            await _recordRepository.CommitTransactionAsync();

            _logger.LogInformation("Batch send completed: {Count} messages, {Success} succeeded, {Failed} failed",
                records.Count,
                records.Count(r => r.Status == MessageStatus.Sent),
                records.Count(r => r.Status == MessageStatus.Failed));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch send failed, rolling back transaction");
            await _recordRepository.RollbackTransactionAsync();
            throw;
        }

        return records.Select(ToDto).ToList();
    }

    public async Task<MessageRecordDto?> GetRecordAsync(Guid id)
    {
        var record = await _recordRepository.FindAsync(id);
        return record != null ? ToDto(record) : null;
    }

    public async Task<(List<MessageRecordDto> Items, int Total)> GetRecordsAsync(
        string? channel = null,
        string? status = null,
        string? receiver = null,
        int page = 1,
        int pageSize = 20)
    {
        MessageChannel? channelEnum = null;
        if (!string.IsNullOrEmpty(channel) && Enum.TryParse<MessageChannel>(channel, true, out var parsed))
        {
            channelEnum = parsed;
        }

        MessageStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MessageStatus>(status, true, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        var result = await _recordRepository.FindListAsync(
            x => (channelEnum == null || x.Channel == channelEnum) &&
                 (statusEnum == null || x.Status == statusEnum) &&
                 (string.IsNullOrEmpty(receiver) || x.Receiver.Contains(receiver)),
            pageIndex: page,
            pageSize: pageSize,
            orderBy: x => x.CreationTime,
            orderByDescending: true);

        return (result.Items.Select(ToDto).ToList(), result.Total);
    }

    public async Task<MessageRecordDto> RetryAsync(Guid id)
    {
        var record = await _recordRepository.FindAsync(id);
        if (record == null)
        {
            throw new KeyNotFoundException($"Message record with id '{id}' not found");
        }

        if (record.Status != MessageStatus.Failed)
        {
            throw new InvalidOperationException("Only failed messages can be retried");
        }

        record.Status = MessageStatus.Pending;
        record.RetryCount++;
        record.FailureReason = null;
        await _recordRepository.UpdateAsync(record);

        await SendMessageInternal(record);

        return ToDto(record);
    }

    public async Task<bool> CancelAsync(Guid id)
    {
        var record = await _recordRepository.FindAsync(id);
        if (record == null)
        {
            return false;
        }

        if (record.Status == MessageStatus.Sent || record.Status == MessageStatus.Delivered)
        {
            return false;
        }

        record.Status = MessageStatus.Cancelled;
        await _recordRepository.UpdateAsync(record);

        _logger.LogInformation("Message {RecordId} cancelled", id);
        return true;
    }

    private async Task SendMessageInternal(MessageRecord record)
    {
        try
        {
            record.Status = MessageStatus.Sending;
            await _recordRepository.UpdateAsync(record);

            var result = record.Channel switch
            {
                MessageChannel.Email => await SendEmailAsync(record),
                MessageChannel.Sms => await SendSmsAsync(record),
                MessageChannel.Push => await SendPushAsync(record),
                _ => throw new ArgumentException($"Unknown channel: {record.Channel}")
            };

            if (result.Success)
            {
                record.Status = MessageStatus.Sent;
                record.SentAt = DateTime.UtcNow;
                record.ExternalId = result.MessageId;
                _logger.LogInformation("Message {RecordId} sent successfully via {Provider}", record.Id, record.Provider);
            }
            else
            {
                record.Status = MessageStatus.Failed;
                record.FailureReason = result.ErrorMessage;
                _logger.LogWarning("Message {RecordId} failed: {Error}", record.Id, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message {RecordId}", record.Id);
            record.Status = MessageStatus.Failed;
            record.FailureReason = ex.Message;
            record.RetryCount++;
        }

        await _recordRepository.UpdateAsync(record);
    }

    private async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendEmailAsync(MessageRecord record)
    {
        var sender = _emailSenders.FirstOrDefault(x =>
            x.ProviderName.Equals(record.Provider, StringComparison.OrdinalIgnoreCase));

        if (sender == null)
        {
            if (_emailSenders.Any())
            {
                sender = _emailSenders.First();
                _logger.LogWarning("Email provider '{Provider}' not found, using default '{Default}'",
                    record.Provider, sender.ProviderName);
            }
            else
            {
                return (false, null, "No email sender configured");
            }
        }

        var content = await RenderTemplateAsync(record);
        var result = await sender.SendAsync(record.Receiver, record.Subject ?? "", content);

        return (result.Success, result.MessageId, result.ErrorMessage);
    }

    private async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendSmsAsync(MessageRecord record)
    {
        var sender = _smsSenders.FirstOrDefault(x =>
            x.ProviderName.Equals(record.Provider, StringComparison.OrdinalIgnoreCase));

        if (sender == null)
        {
            if (_smsSenders.Any())
            {
                sender = _smsSenders.First();
                _logger.LogWarning("SMS provider '{Provider}' not found, using default '{Default}'",
                    record.Provider, sender.ProviderName);
            }
            else
            {
                return (false, null, "No SMS sender configured");
            }
        }

        var templateCode = record.TemplateCode ?? _settings.Sms.DefaultTemplateCode;
        Dictionary<string, string>? templateParams = null;

        if (!string.IsNullOrEmpty(record.Variables))
        {
            try
            {
                templateParams = JsonSerializer.Deserialize<Dictionary<string, string>>(record.Variables);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize variables for record {RecordId}", record.Id);
            }
        }

        var result = await sender.SendAsync(record.Receiver, templateCode, templateParams);

        return (result.Success, result.MessageId, result.ErrorMessage ?? result.ErrorCode);
    }

    private async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendPushAsync(MessageRecord record)
    {
        var sender = _pushSenders.FirstOrDefault(x =>
            x.ProviderName.Equals(record.Provider, StringComparison.OrdinalIgnoreCase));

        if (sender == null)
        {
            if (_pushSenders.Any())
            {
                sender = _pushSenders.First();
                _logger.LogWarning("Push provider '{Provider}' not found, using default '{Default}'",
                    record.Provider, sender.ProviderName);
            }
            else
            {
                return (false, null, "No push sender configured");
            }
        }

        var content = await RenderTemplateAsync(record);

        var request = new PushRequest
        {
            Title = record.Subject ?? "",
            Content = content,
            Extras = !string.IsNullOrEmpty(record.Variables)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(record.Variables)
                : null
        };

        var result = await sender.SendAsync(request);

        return (result.Success, result.MessageId, result.ErrorMessage);
    }

    private async Task<string> RenderTemplateAsync(MessageRecord record)
    {
        if (string.IsNullOrEmpty(record.TemplateCode))
        {
            return record.Content;
        }

        var template = await _templateRepository.FindAsync(
            x => x.Code == record.TemplateCode && x.Channel == record.Channel);

        if (template == null)
        {
            return record.Content;
        }

        var content = template.ContentTemplate;
        if (!string.IsNullOrEmpty(record.Variables))
        {
            try
            {
                var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(record.Variables);
                if (variables != null)
                {
                    foreach (var kvp in variables)
                    {
                        content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to render template for record {RecordId}", record.Id);
            }
        }

        return content;
    }

    private string GetDefaultProvider(MessageChannel channel)
    {
        return channel switch
        {
            MessageChannel.Email => _settings.Email.DefaultProvider,
            MessageChannel.Sms => _settings.Sms.DefaultProvider,
            MessageChannel.Push => _settings.Push.DefaultProvider,
            _ => "unknown"
        };
    }

    private static MessageRecordDto ToDto(MessageRecord record)
    {
        Dictionary<string, string>? variables = null;
        if (!string.IsNullOrEmpty(record.Variables))
        {
            try
            {
                variables = JsonSerializer.Deserialize<Dictionary<string, string>>(record.Variables);
            }
            catch
            {
                // Variables deserialization failed, return null
            }
        }

        return new MessageRecordDto
        {
            Id = record.Id,
            Channel = record.Channel.ToString(),
            TemplateId = record.TemplateId,
            TemplateCode = record.TemplateCode,
            Receiver = record.Receiver,
            Subject = record.Subject,
            Content = record.Content,
            Variables = variables,
            Provider = record.Provider,
            Status = record.Status.ToString(),
            ExternalId = record.ExternalId,
            FailureReason = record.FailureReason,
            RetryCount = record.RetryCount,
            SentAt = record.SentAt,
            DeliveredAt = record.DeliveredAt,
            ScheduledAt = record.ScheduledAt,
            SenderId = record.SenderId,
            BusinessId = record.BusinessId,
            BusinessType = record.BusinessType,
            CreationTime = record.CreationTime
        };
    }
}
