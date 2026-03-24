using Orleans;

namespace Stargazer.Orleans.MessageManagement.Grains.Abstractions.Messages;

public interface IScheduledMessageReminderGrain : IGrainWithStringKey
{
    Task RegisterReminderAsync(Guid messageId, DateTime scheduledAt);
    Task UnregisterReminderAsync(Guid messageId);
}
