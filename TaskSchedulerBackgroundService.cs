using JobScheduler.Interfaces;
using Microsoft.Extensions.Options;
using NCrontab;

namespace JobScheduler;

public class TaskSchedulerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ScheduledTaskConfig> _jobConfigs;
    private readonly Dictionary<string, CrontabSchedule> _schedules = new();
    private readonly Dictionary<string, DateTime> _nextRuns = new();
    private bool firstRun = true;

    public TaskSchedulerBackgroundService(IOptions<List<ScheduledTaskConfig>> options, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _jobConfigs = options.Value;

        foreach (var job in _jobConfigs.Where(j => j.Enabled))
        {
            var schedule = CrontabSchedule.Parse(job.Cron);
            _schedules[job.Name] = schedule;
            _nextRuns[job.Name] = schedule.GetNextOccurrence(ThisMinute());
        }
    }

    private DateTime ThisMinute()
    {
        var now = DateTime.Now;
        return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
    }


    async Task DelayUntilNextWholeMinuteAsync(CancellationToken cancellationToken)
    {
        var delay = ThisMinute().AddMinutes(1) - DateTime.Now;
        await Task.Delay(delay, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = ThisMinute();

            foreach (var jobConfig in _jobConfigs.Where(j => j.Enabled))
            {
                if (_nextRuns[jobConfig.Name] <= now || firstRun)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var job = scope.ServiceProvider
                        .GetServices<ITimeTriggeredJob>()
                        .FirstOrDefault(j => j.Name == jobConfig.Name);

                    if (job != null)
                    {
                        var _schedule = _schedules[jobConfig.Name];
                        var next = _schedule.GetNextOccurrence(_nextRuns[jobConfig.Name]);
                        _nextRuns[jobConfig.Name] = next;
                        job.NextOccurrence = _nextRuns[jobConfig.Name].ToString("HH:mm:ss");
                        _ = Task.Run(() => job.ExecuteAsync(stoppingToken), stoppingToken);
                    }
                }
            }


            firstRun = false;
            await DelayUntilNextWholeMinuteAsync(stoppingToken);
        }
    }
}