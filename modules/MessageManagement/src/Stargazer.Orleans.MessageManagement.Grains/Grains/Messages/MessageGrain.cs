using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Stargazer.Orleans.MessageManagement.Domain;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions;
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
public partial class MessageGrain : Grain, IMessageGrain
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
        var channel = input.Channel;
        Guid? templateId = null;

        if (!string.IsNullOrEmpty(input.TemplateCode))
        {
            var template = await _templateRepository.FindAsync(
                x => x.Code == input.TemplateCode && x.Channel == channel);
            if (template != null)
            {
                templateId = template.Id;
            }
            else
            {
                _logger.LogWarning("Template code {TemplateCode} not found for channel {Channel}",
                    input.TemplateCode, channel);
            }
        }

        var record = new MessageRecord
        {
            Id = Guid.NewGuid(),
            Channel = channel,
            TemplateId = templateId,
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
            _logger.LogInformation("Message {RecordId} scheduled for {ScheduledAt}, registering reminder",
                record.Id, input.ScheduledAt);

            var reminderGrain = GrainFactory.GetGrain<IScheduledMessageReminderGrain>(GetReminderGrainKey(input.ScheduledAt.Value));
            await reminderGrain.RegisterReminderAsync(record.Id, input.ScheduledAt.Value);

            return ToDto(record);
        }

        await SendMessageInternal(record);

        return ToDto(record);
    }

    private static string GetReminderGrainKey(DateTime scheduledAt)
    {
        return $"scheduler_{scheduledAt:yyyyMMddHH}";
    }

    public async Task<List<MessageRecordDto>> BatchSendAsync(BatchSendMessageInputDto input)
    {
        var channel = input.Channel;
        var records = new List<MessageRecord>(input.Receivers.Count);

        foreach (var receiver in input.Receivers)
        {
            records.Add(new MessageRecord
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
            });
        }

        try
        {
            await _recordRepository.BeginTransactionAsync();
            await _recordRepository.InsertAsync(records);
            await _recordRepository.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert batch records, rolling back");
            await _recordRepository.RollbackTransactionAsync();
            throw;
        }

        var sendTasks = records.Select(SendMessageInternal).ToList();
        await Task.WhenAll(sendTasks);

        _logger.LogInformation("Batch send completed: {Count} messages, {Success} succeeded, {Failed} failed",
            records.Count,
            records.Count(r => r.Status == MessageStatus.Sent),
            records.Count(r => r.Status == MessageStatus.Failed));

        return records.Select(ToDto).ToList();
    }

    public async Task<MessageRecordDto?> GetRecordAsync(Guid id)
    {
        var record = await _recordRepository.FindAsync(id);
        return record != null ? ToDto(record) : null;
    }

    public async Task<PageResult<MessageRecordDto>> GetRecordsAsync(
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

        return new PageResult<MessageRecordDto>
        {
            Total = result.Total,
            Items = result.Items.Select(ToDto).ToList()
        };
    }

    public async Task<MessageRecordDto> RetryAsync(Guid id)
    {
        var record = await _recordRepository.FindAsync(id);
        if (record == null)
        {
            throw new KeyNotFoundException("record_not_found");
        }

        if (record.Status != MessageStatus.Failed)
        {
            throw new InvalidOperationException("only_failed_can_retry");
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

        if (record.Status is MessageStatus.Sent or MessageStatus.Delivered)
        {
            _logger.LogWarning("Cannot cancel message {RecordId}: status is {Status}", id, record.Status);
            return false;
        }

        if (record.ScheduledAt.HasValue && record.ScheduledAt > DateTime.UtcNow)
        {
            var reminderGrain = GrainFactory.GetGrain<IScheduledMessageReminderGrain>(
                GetReminderGrainKey(record.ScheduledAt.Value));
            await reminderGrain.UnregisterReminderAsync(record.Id);
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
        var sender = GetEmailSender(record.Provider);
        if (sender == null)
        {
            return (false, null, "No email sender configured");
        }

        var content = await RenderTemplateAsync(record);
        var result = await sender.SendAsync(record.Receiver, record.Subject ?? "", content);

        return (result.Success, result.MessageId, result.ErrorMessage);
    }

    private async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendSmsAsync(MessageRecord record)
    {
        var sender = GetSmsSender(record.Provider);
        if (sender == null)
        {
            return (false, null, "No SMS sender configured");
        }

        var templateCode = record.TemplateCode ?? _settings.Sms.DefaultTemplateCode;
        var templateParams = ParseVariables(record.Variables);

        var result = await sender.SendAsync(record.Receiver, templateCode, templateParams);

        return (result.Success, result.MessageId, result.ErrorMessage ?? result.ErrorCode);
    }

    private async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendPushAsync(MessageRecord record)
    {
        var sender = GetPushSender(record.Provider);
        if (sender == null)
        {
            return (false, null, "No push sender configured");
        }

        var content = await RenderTemplateAsync(record);

        var request = new PushRequest
        {
            Title = record.Subject ?? "",
            Content = content,
            Extras = ParseVariables(record.Variables)
        };

        var result = await sender.SendAsync(request);

        return (result.Success, result.MessageId, result.ErrorMessage);
    }

    private IEmailSender? GetEmailSender(string? providerName)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            return _emailSenders.FirstOrDefault();
        }

        var sender = _emailSenders.FirstOrDefault(x =>
            x.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (sender == null)
        {
            var defaultSender = _emailSenders.FirstOrDefault();
            if (defaultSender != null)
            {
                _logger.LogWarning("Email provider '{Provider}' not found, using default '{Default}'",
                    providerName, defaultSender.ProviderName);
            }
            return defaultSender;
        }

        return sender;
    }

    private ISmsSender? GetSmsSender(string? providerName)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            return _smsSenders.FirstOrDefault();
        }

        var sender = _smsSenders.FirstOrDefault(x =>
            x.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (sender == null)
        {
            var defaultSender = _smsSenders.FirstOrDefault();
            if (defaultSender != null)
            {
                _logger.LogWarning("SMS provider '{Provider}' not found, using default '{Default}'",
                    providerName, defaultSender.ProviderName);
            }
            return defaultSender;
        }

        return sender;
    }

    private IPushSender? GetPushSender(string? providerName)
    {
        if (string.IsNullOrEmpty(providerName))
        {
            return _pushSenders.FirstOrDefault();
        }

        var sender = _pushSenders.FirstOrDefault(x =>
            x.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (sender == null)
        {
            var defaultSender = _pushSenders.FirstOrDefault();
            if (defaultSender != null)
            {
                _logger.LogWarning("Push provider '{Provider}' not found, using default '{Default}'",
                    providerName, defaultSender.ProviderName);
            }
            return defaultSender;
        }

        return sender;
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

        var variables = ParseVariables(record.Variables);
        if (variables == null || variables.Count == 0)
        {
            return template.ContentTemplate;
        }

        return VariableRegex().Replace(template.ContentTemplate, match =>
        {
            var key = match.Groups["key"].Value;
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    private static Dictionary<string, string>? ParseVariables(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch (JsonException)
        {
            return null;
        }
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

    private MessageRecordDto ToDto(MessageRecord record)
    {
        return new MessageRecordDto
        {
            Id = record.Id,
            Channel = record.Channel.ToString(),
            TemplateId = record.TemplateId,
            TemplateCode = record.TemplateCode,
            Receiver = record.Receiver,
            Subject = record.Subject,
            Content = record.Content,
            Variables = ParseVariables(record.Variables),
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

    [GeneratedRegex(@"\{\{(?<key>\w+)\}\}")]
    private static partial Regex VariableRegex();
}
