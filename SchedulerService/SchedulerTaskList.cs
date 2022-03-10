namespace CoreLibrary.SchedulerService
{
    public class SchedulerTaskList : List<Func<CancellationToken, Task>>
    {
    }
}
