namespace CoreLibrary.SchedulerService
{
    public class DefaultSchedulerConfig<TKey> : ISchedulerConfig<object, TKey> where TKey : notnull
    {
        public Dictionary<TKey, List<ScheduledTaskDelegate<object>>> Tasks => new();
    }
}
