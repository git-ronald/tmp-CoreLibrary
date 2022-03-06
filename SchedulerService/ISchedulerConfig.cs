namespace CoreLibrary.SchedulerService
{
    public interface ISchedulerConfig<TKey> where TKey : notnull
    {
        Dictionary<TKey, List<CancellableTaskDelegate>> Tasks { get; }
    }
}
