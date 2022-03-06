namespace CoreLibrary.SchedulerService
{
    public interface ISchedulerConfig<TState, TKey> where TKey : notnull
    {
        Dictionary<TKey, List<ScheduledTaskDelegate<TState>>> Tasks { get; }
    }
}
