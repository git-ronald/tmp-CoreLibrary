using CoreLibrary.Helpers;
using System.Diagnostics;

namespace CoreLibrary.SchedulerService
{
    /// <summary>
    /// Schedules tasks with consistent time compartments
    /// TODO NOW:  move eryerthing to CoreLibrary, so LocalClient can use it.
    /// </summary>
    public class SchedulerService : ISchedulerService
    {
        private readonly ISchedulerConfig<TimeSpan> _fixedTimeSchedule;
        private readonly ISchedulerConfig<TimeCompartments> _timeCompartmentSchedule;
        private readonly Dictionary<TimeCompartments, DateTime> _nextCompartmentEvents;

        public SchedulerService(ISchedulerConfig<TimeSpan> fixedTimeSchedule, ISchedulerConfig<TimeCompartments> scheduleConfig)
        {
            _fixedTimeSchedule = fixedTimeSchedule;
            _timeCompartmentSchedule = scheduleConfig;

            _nextCompartmentEvents = _timeCompartmentSchedule.Tasks.Keys.ToDictionary(tc => tc, _ => DateTime.MinValue);
        }

        public async Task Start(CancellationToken stoppingToken)
        {
            try
            {
                while (true)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    DateTime now = DateTime.UtcNow;
                    CalculateNextEventsForCompartiments(now);

                    var (earliesDateTime, fixedTimeEvent) = GetNextEvent(now);

                    if (!earliesDateTime.HasValue)
                    {
                        break;
                    }

                    int delay = (int)(earliesDateTime.Value - DateTime.UtcNow).TotalMilliseconds;
                    if (delay > 0)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }

                    if (fixedTimeEvent.HasValue)
                    {
                        await ExecuteTaskList(stoppingToken, _fixedTimeSchedule.Tasks[fixedTimeEvent.Value]);
                    }

                    foreach (var compartment in _timeCompartmentSchedule.Tasks.Keys)
                    {
                        await ExecuteTasksForCompartment(stoppingToken, compartment);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: logging!
            }
        }

        private void CalculateNextEventsForCompartiments(DateTime now)
        {
            foreach (TimeCompartments compartment in _nextCompartmentEvents.Keys)
            {
                _nextCompartmentEvents[compartment] = now.CalcNextNthMinute((int)compartment);
            }
        }

        private (DateTime?, TimeSpan?) GetNextEvent(DateTime now)
        {
            IEnumerable<(DateTime DateTime, TimeSpan? TimeSpan)> GetEarliestCandidates()
            {
                var fixedTimeEvents = GetNextEventsForFixedTime(now).OrderBy(fte => fte.DateTime);

                if (fixedTimeEvents.Any())
                {
                    yield return fixedTimeEvents.First();
                }

                if (_nextCompartmentEvents.Count > 0)
                {
                    yield return _nextCompartmentEvents.Values.OrderBy(dt => dt).Select(dt => (dt, new TimeSpan?())).First();
                }
            }

            var earliestCandidates = GetEarliestCandidates().GroupBy(e => e.DateTime).OrderBy(g => g.Key);
            if (!earliestCandidates.Any())
            {
                return (null, null);
            }

            var earliestEvent = earliestCandidates.First();
            DateTime earliestDateTime = earliestEvent.Key;
            TimeSpan? fixedTimeEvent = earliestEvent.Select(e => e.TimeSpan).FirstOrDefault(ts => ts.HasValue);

            return (earliestDateTime, fixedTimeEvent);
        }

        private IEnumerable<(DateTime DateTime, TimeSpan? TimeSpan)> GetNextEventsForFixedTime(DateTime now)
        {
            foreach (TimeSpan time in _fixedTimeSchedule.Tasks.Keys)
            {
                DateTime forToday = new DateTime(now.Year, now.Month, now.Day, time.Hours, time.Minutes, 0, DateTimeKind.Utc);
                if (forToday > now)
                {
                    yield return (forToday, time);
                }
                else
                {
                    yield return (forToday.AddDays(1), time);
                }
            }
        }

        private async Task ExecuteTasksForCompartment(CancellationToken stoppingToken, TimeCompartments compartment)
        {
            var now = DateTime.UtcNow;
            if (_nextCompartmentEvents[compartment] > now)
            {
                return; // Scheduled event is still in the future
            }

            _nextCompartmentEvents[compartment] = now.CalcNextNthMinute((int)compartment); // Schedule next event

            var tasksForCompartment = _timeCompartmentSchedule.Tasks[compartment];
            await ExecuteTaskList(stoppingToken, tasksForCompartment);
        }

        private async Task ExecuteTaskList(CancellationToken stoppingToken, IEnumerable<CancellableTaskDelegate> taskList)
        {
            foreach (CancellableTaskDelegate execute in taskList)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await execute(stoppingToken);
                }
                catch (Exception ex)
                {
                    // TODO: logging!
                    Debug.WriteLine($"ERROR: {ex.Message}");
                }
            }

        }
    }
}
