namespace CoreLibrary.SchedulerService
{
    public interface ISchedulerService
    {
        Task Start(CancellationToken stoppingToken, IDictionary<TimeSpan, SchedulerTaskList>? fixedTimeSchedule = null, IDictionary<TimeCompartments, SchedulerTaskList>? compartmentSchedule = null);
    }
}