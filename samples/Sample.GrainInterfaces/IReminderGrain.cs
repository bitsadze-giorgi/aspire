namespace Sample.GrainInterfaces;

public interface IReminderGrain : IGrainWithStringKey
{
    Task<DateTimeOffset?> GetLastTick();
    Task Start(TimeSpan period);
    Task Stop();
}
