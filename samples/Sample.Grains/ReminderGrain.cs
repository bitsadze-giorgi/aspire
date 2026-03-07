using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Sample.GrainInterfaces;

namespace Sample.Grains;

public class ReminderGrain(ILogger<ReminderGrain> logger) : Grain, IReminderGrain, IRemindable
{
    private IGrainReminder? _reminder;
    private DateTimeOffset? _lastTick;

    public Task<DateTimeOffset?> GetLastTick() => Task.FromResult(_lastTick);

    public async Task Start(TimeSpan period)
    {
        _reminder = await this.RegisterOrUpdateReminder("tick", dueTime: TimeSpan.Zero, period: period);
        logger.LogInformation("Reminder started for {GrainId} with period {Period}", this.GetPrimaryKeyString(), period);
    }

    public async Task Stop()
    {
        if (_reminder is not null)
        {
            await this.UnregisterReminder(_reminder);
            _reminder = null;
            logger.LogInformation("Reminder stopped for {GrainId}", this.GetPrimaryKeyString());
        }
    }

    public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        _lastTick = DateTimeOffset.UtcNow;
        logger.LogInformation("Reminder tick for {GrainId} at {Time}", this.GetPrimaryKeyString(), _lastTick);
        return Task.CompletedTask;
    }
}
