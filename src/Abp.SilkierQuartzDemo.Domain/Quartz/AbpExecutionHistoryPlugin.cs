using System;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Plugins.RecentHistory;
using Quartz.Spi;
using Volo.Abp;

namespace Abp.SilkierQuartzDemo.Quartz;

public class AbpExecutionHistoryPlugin : ISchedulerPlugin, IJobListener
{
    private IScheduler _scheduler = null!;
    private IExecutionHistoryStore _store = null!;

    public Type StoreType { get; set; } = null!;

    public string Name { get; protected set; } = string.Empty;

    public Task Initialize(string pluginName, IScheduler scheduler, CancellationToken cancellationToken = default)
    {
        Name = pluginName;
        _scheduler = scheduler;
        _scheduler.ListenerManager.AddJobListener(this, EverythingMatcher<JobKey>.AllJobs());

        return Task.FromResult(0);
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        _store = _scheduler.Context.GetExecutionHistoryStore();

        if (_store == null)
        {
            throw new AbpException(nameof(StoreType) + " is not set.");
        }

        _store.SchedulerName = _scheduler.SchedulerName;

        if (_store is AbpExecutionHistoryStore abpStore)
            await abpStore.InitializeSummaryAsync();

        await _store.Purge();
    }

    public Task Shutdown(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var entry = new ExecutionHistoryEntry()
        {
            FireInstanceId = context.FireInstanceId,
            SchedulerInstanceId = context.Scheduler.SchedulerInstanceId,
            SchedulerName = context.Scheduler.SchedulerName,
            ActualFireTimeUtc = context.FireTimeUtc.UtcDateTime,
            ScheduledFireTimeUtc = context.ScheduledFireTimeUtc?.UtcDateTime,
            Recovering = context.Recovering,
            Job = context.JobDetail.Key.ToString(),
            Trigger = context.Trigger.Key.ToString(),
        };
        await _store.Save(entry);
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        var entry = await _store.Get(context.FireInstanceId);
        if (entry != null)
        {
            entry.FinishedTimeUtc = DateTime.UtcNow;
            entry.ExceptionMessage = jobException?.GetBaseException()?.ToString();
        }
        else
        {
            entry = new ExecutionHistoryEntry()
            {
                FireInstanceId = context.FireInstanceId,
                SchedulerInstanceId = context.Scheduler.SchedulerInstanceId,
                SchedulerName = context.Scheduler.SchedulerName,
                ActualFireTimeUtc = context.FireTimeUtc.UtcDateTime,
                ScheduledFireTimeUtc = context.ScheduledFireTimeUtc?.UtcDateTime,
                Recovering = context.Recovering,
                Job = context.JobDetail.Key.ToString(),
                Trigger = context.Trigger.Key.ToString(),
                FinishedTimeUtc = DateTime.UtcNow,
                ExceptionMessage = jobException?.GetBaseException()?.ToString()
            };
        }
        await _store.Save(entry);

        if (jobException == null)
            await _store.IncrementTotalJobsExecuted();
        else
            await _store.IncrementTotalJobsFailed();
    }

    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var entry = await _store.Get(context.FireInstanceId);
        if (entry != null)
        {
            entry.Vetoed = true;
            await _store.Save(entry);
        }
    }
}
