using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Reminders;
using Orleans.Timers;
using Stargazer.Orleans.MessageManagement.Domain;
using Stargazer.Orleans.MessageManagement.Domain.Shared;
using Stargazer.Orleans.MessageManagement.EntityFrameworkCore.PostgreSQL;
using Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Email;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Push;
using Stargazer.Orleans.MessageManagement.Grains.Senders.Sms;

namespace Stargazer.Orleans.MessageManagement.Grains.Grains.Messages;

public class ScheduledMessageReminderGrain : IGrainBase, IScheduledMessageReminderGrain, IRemindable, IDisposable
{
    private readonly IRepository<MessageRecord, Guid> _recordRepository;
    private readonly IEnumerable<IEmailSender> _emailSenders;
    private readonly IEnumerable<ISmsSender> _smsSenders;
    private readonly IEnumerable<IPushSender> _pushSenders;
    private readonly IReminderRegistry _reminderRegistry;
    private readonly ITimerRegistry _timerRegistry;
    private readonly IGrainContext _grainContext;
    private readonly ILogger<ScheduledMessageReminderGrain> _logger;
    private readonly ConcurrentDictionary<string, IGrainReminder> _reminders = new();

    public ScheduledMessageReminderGrain(
        IRepository<MessageRecord, Guid> recordRepository,
        IEnumerable<IEmailSender> emailSenders,
        IEnumerable<ISmsSender> smsSenders,
        IEnumerable<IPushSender> pushSenders,
        IReminderRegistry reminderRegistry,
        ITimerRegistry timerRegistry,
        IGrainContext grainContext,
        ILogger<ScheduledMessageReminderGrain> logger)
    {
        _recordRepository = recordRepository;
        _emailSenders = emailSenders;
        _smsSenders = smsSenders;
        _pushSenders = pushSenders;
        _reminderRegistry = reminderRegistry;
        _timerRegistry = timerRegistry;
        _grainContext = grainContext;
        _logger = logger;
    }

    public IGrainContext GrainContext => _grainContext;

    public async Task RegisterReminderAsync(Guid messageId, DateTime scheduledAt)
    {
        var reminderName = GetReminderName(messageId);
        var dueTime = scheduledAt > DateTime.UtcNow ? scheduledAt - DateTime.UtcNow : TimeSpan.Zero;

        _logger.LogInformation("Registering reminder for message {MessageId} at {ScheduledAt}, due in {DueTime}",
            messageId, scheduledAt, dueTime);

        var reminder = await _reminderRegistry.RegisterOrUpdateReminder(
            _grainContext.GrainId,
            reminderName,
            dueTime,
            TimeSpan.FromMinutes(5));

        _reminders.TryAdd(reminderName, reminder);
    }

    public async Task UnregisterReminderAsync(Guid messageId)
    {
        var reminderName = GetReminderName(messageId);

        if (_reminders.TryRemove(reminderName, out var reminder))
        {
            await _reminderRegistry.UnregisterReminder(_grainContext.GrainId, reminder);
            _logger.LogInformation("Unregistered reminder for message {MessageId}", messageId);
        }
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        var messageId = ParseMessageId(reminderName);
        if (messageId == null)
        {
            _logger.LogWarning("Invalid reminder name: {ReminderName}", reminderName);
            return;
        }

        _logger.LogInformation("Processing reminder for message {MessageId}", messageId);

        var record = await _recordRepository.FindAsync(messageId.Value);
        if (record == null)
        {
            _logger.LogWarning("Scheduled message {MessageId} not found, unregistering reminder", messageId);
            await UnregisterReminderAsync(messageId.Value);
            return;
        }

        if (record.Status != MessageStatus.Pending)
        {
            _logger.LogInformation("Message {MessageId} is no longer pending (status: {Status}), unregistering reminder",
                messageId, record.Status);
            await UnregisterReminderAsync(messageId.Value);
            return;
        }

        await ProcessScheduledMessageAsync(record);
        await UnregisterReminderAsync(messageId.Value);
    }

    private async Task ProcessScheduledMessageAsync(MessageRecord record)
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
                _ => (false, null, $"Unknown channel: {record.Channel}")
            };

            if (result.Success)
            {
                record.Status = MessageStatus.Sent;
                record.SentAt = DateTime.UtcNow;
                record.ExternalId = result.MessageId;
                _logger.LogInformation("Scheduled message {RecordId} sent successfully", record.Id);
            }
            else
            {
                record.Status = MessageStatus.Failed;
                record.FailureReason = result.ErrorMessage;
                _logger.LogWarning("Scheduled message {RecordId} failed: {Error}", record.Id, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process scheduled message {RecordId}", record.Id);
            record.Status = MessageStatus.Failed;
            record.FailureReason = ex.Message;
        }

        await _recordRepository.UpdateAsync(record);
    }

    private async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendEmailAsync(MessageRecord record)
    {
        var sender = _emailSenders.FirstOrDefault(x =>
            x.ProviderName.Equals(record.Provider, StringComparison.OrdinalIgnoreCase))
            ?? _emailSenders.FirstOrDefault();

        if (sender == null)
            return (false, null, "No email sender configured");

        var content = await RenderTemplateAsync(record);
        var result = await sender.SendAsync(record.Receiver, record.Subject ?? "", content);
        return (result.Success, result.MessageId, result.ErrorMessage);
    }

    private async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendSmsAsync(MessageRecord record)
    {
        var sender = _smsSenders.FirstOrDefault(x =>
            x.ProviderName.Equals(record.Provider, StringComparison.OrdinalIgnoreCase))
            ?? _smsSenders.FirstOrDefault();

        if (sender == null)
            return (false, null, "No SMS sender configured");

        Dictionary<string, string>? templateParams = null;
        if (!string.IsNullOrEmpty(record.Variables))
        {
            try
            {
                templateParams = JsonSerializer.Deserialize<Dictionary<string, string>>(record.Variables);
            }
            catch { }
        }

        var result = await sender.SendAsync(record.Receiver, record.TemplateCode ?? "", templateParams);
        return (result.Success, result.MessageId, result.ErrorMessage ?? result.ErrorCode);
    }

    private async Task<(bool Success, string? MessageId, string? ErrorMessage)> SendPushAsync(MessageRecord record)
    {
        var sender = _pushSenders.FirstOrDefault(x =>
            x.ProviderName.Equals(record.Provider, StringComparison.OrdinalIgnoreCase))
            ?? _pushSenders.FirstOrDefault();

        if (sender == null)
            return (false, null, "No push sender configured");

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
        if (string.IsNullOrEmpty(record.Variables))
            return record.Content ?? "";

        try
        {
            var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(record.Variables);
            if (variables != null)
            {
                var content = record.Content ?? "";
                foreach (var kvp in variables)
                {
                    content = content.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                }
                return content;
            }
        }
        catch { }

        return record.Content ?? "";
    }

    private static string GetReminderName(Guid messageId) => $"scheduled_{messageId}";

    private static Guid? ParseMessageId(string reminderName)
    {
        if (reminderName.StartsWith("scheduled_") && Guid.TryParse(reminderName[10..], out var id))
            return id;
        return null;
    }

    public void Dispose()
    {
        _reminders.Clear();
    }
}
