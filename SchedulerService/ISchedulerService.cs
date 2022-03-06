namespace CoreLibrary.SchedulerService
{
    public interface ISchedulerService : ISchedulerService<object>
    {
    }
    public interface ISchedulerService<TState>
    {
        Task Start(CancellationToken stoppingToken, TState? state = default(TState));
    }
}