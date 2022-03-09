using CoreLibrary.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CoreLibrary.SchedulerService
{
    /// <summary>
    /// Schedules tasks with consistent time compartments
    /// </summary>
    public class SchedulerService : ISchedulerService
    {
        private IDictionary<TimeSpan, SchedulerTaskList> _fixedTimeSchedule = new Dictionary<TimeSpan, SchedulerTaskList>();
        private IDictionary<TimeCompartments, SchedulerTaskList> _compartmentSchedule = new Dictionary<TimeCompartments, SchedulerTaskList>();

        private Dictionary<TimeCompartments, DateTime> _nextCompartmentEvents = new();

        //private TState? _state = default(TState);

        //public SchedulerService(ISchedulerConfig<TState, TimeSpan> fixedTimeSchedule, ISchedulerConfig<TState, TimeCompartments> scheduleConfig)
        //{
        //    _fixedTimeSchedule = fixedTimeSchedule;
        //    _timeCompartmentSchedule = scheduleConfig;

        //    _nextCompartmentEvents = _timeCompartmentSchedule.Tasks.Keys.ToDictionary(tc => tc, _ => DateTime.MinValue);
        //}

        public async Task Start(CancellationToken stoppingToken, IDictionary<TimeSpan, SchedulerTaskList>? fixedTimeSchedule = null, IDictionary<TimeCompartments, SchedulerTaskList>? compartmentSchedule = null) //, TState? state = default(TState))
        {
            try
            {
                if (fixedTimeSchedule is not null)
                {
                    _fixedTimeSchedule = fixedTimeSchedule;
                }
                if (compartmentSchedule is not null)
                {
                    _compartmentSchedule = compartmentSchedule;
                }

                while (true)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    DateTime now = DateTime.UtcNow;
                    _nextCompartmentEvents = CalculateNextEventsForCompartiments(now);
                    //_nextCompartmentEvents = _compartmentSchedule.Keys.ToDictionary(tc => tc, _ => DateTime.MinValue);

                    //CalculateNextEventsForCompartiments(now);

                    // TODO: fixed time events needs to have an "executed" flag.
                    // Then the earliest not-executed events needs to be retrieved by GetNextEvent. This could produce a negative delay.
                    // This way all fixed time events are guaranteed to be executed.

                    var (earliesDateTime, fixedTimeEvent) = GetNextEvent(now);
                    if (!earliesDateTime.HasValue)
                    {
                        break; // Absolutely nothing is scheduled so nvm...
                    }

                    int delay = (int)(earliesDateTime.Value - DateTime.UtcNow).TotalMilliseconds;
                    if (delay > 0)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }

                    if (fixedTimeEvent.HasValue)
                    {
                        await ExecuteTaskList(stoppingToken, _fixedTimeSchedule[fixedTimeEvent.Value]);
                    }

                    foreach (var compartment in _compartmentSchedule.Keys)
                    {
                        await ExecuteTasksForCompartment(stoppingToken, compartment);
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                // Usually it was just an awaited task being cancelled which is fine... unless ex.Task.Exception is not null:
                if (ex.Task?.Exception is not null)
                {
                    // TODO: logging!
                }
            }
            catch (Exception ex)
            {
                // TODO: logging!
            }
        }

        private Dictionary<TimeCompartments, DateTime> CalculateNextEventsForCompartiments(DateTime now)
        {
            IEnumerable<KeyValuePair<TimeCompartments, DateTime>> GetNextEvents()
            {
                foreach (TimeCompartments compartment in _compartmentSchedule.Keys)
                {
                    yield return new KeyValuePair<TimeCompartments, DateTime>(compartment, now.CalcNextNthMinute((int)compartment));
                }
            }

            return new Dictionary<TimeCompartments, DateTime>(GetNextEvents());
        }
        private void CalculateNextEventsForCompartimentsOLD(DateTime now)
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
                IEnumerable<(DateTime DateTime, TimeSpan TimeSpan)> fixedTimeEvents = _fixedTimeSchedule.Keys.Select(time => (now.Apply(time, 0), time));

                if (fixedTimeEvents.Any())
                {
                    //    Console.Write("### ");
                    //    foreach (var item in fixedTimeEvents)
                    //    {
                    //        Console.Write($"{item.DateTime} {item.TimeSpan} - ");
                    //    }
                    //    Console.WriteLine(); 
                    yield return fixedTimeEvents.OrderBy(e => e.DateTime).First();
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
            //Console.WriteLine($"### Earliest next event: {earliestEvent.Key}");
            DateTime earliestDateTime = earliestEvent.Key;
            TimeSpan? fixedTimeEvent = earliestEvent.Select(e => e.TimeSpan).FirstOrDefault(ts => ts.HasValue);

            return (earliestDateTime, fixedTimeEvent);
        }

        //private IEnumerable<(DateTime DateTime, TimeSpan? TimeSpan)> GetNextEventsForFixedTime(DateTime now)
        //{
        //    foreach (TimeSpan time in _fixedTimeConfig.Keys)
        //    {
        //        DateTime forToday = new DateTime(now.Year, now.Month, now.Day, time.Hours, time.Minutes, 0, DateTimeKind.Utc);
        //        if (forToday > now)
        //        {
        //            yield return (forToday, time);
        //        }
        //        else
        //        {
        //            yield return (forToday.AddDays(1), time);
        //        }
        //    }
        //}

        private async Task ExecuteTasksForCompartment(CancellationToken stoppingToken, TimeCompartments compartment)
        {
            var now = DateTime.UtcNow;
            if (_nextCompartmentEvents[compartment] > now)
            {
                return; // Scheduled event is still in the future
            }

            _nextCompartmentEvents[compartment] = now.CalcNextNthMinute((int)compartment); // Schedule next event

            var tasksForCompartment = _compartmentSchedule[compartment];
            await ExecuteTaskList(stoppingToken, tasksForCompartment);
        }

        private async Task ExecuteTaskList(CancellationToken stoppingToken, IEnumerable<Func<CancellationToken, Task>> taskList)
        {
            foreach (Func<CancellationToken, Task> task in taskList)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await task(stoppingToken);
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
