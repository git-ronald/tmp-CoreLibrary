namespace CoreLibrary.SchedulerService
{
    public class DefaultSchedulerConfig<TKey> : ISchedulerConfig<TKey> where TKey : notnull
    {
        public static ISchedulerConfig<TKey> Create() => new DefaultSchedulerConfig<TKey>();
        public Dictionary<TKey, List<CancellableTaskDelegate>> Tasks => new();
    }
}
