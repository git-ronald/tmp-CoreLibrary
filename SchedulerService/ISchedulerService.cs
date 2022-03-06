namespace CoreLibrary.SchedulerService
{
    public interface ISchedulerService
    {
        Task Start(CancellationToken stoppingToken);
    }
}