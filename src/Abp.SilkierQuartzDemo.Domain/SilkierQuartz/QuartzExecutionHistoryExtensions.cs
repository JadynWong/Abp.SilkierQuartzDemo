using Quartz.Plugins.RecentHistory;

namespace Abp.SilkierQuartzDemo.SilkierQuartz;

public static class QuartzExecutionHistoryExtensions
{
    public static QuartzExecutionHistory ToEntity(this ExecutionHistoryEntry entry, QuartzExecutionHistory entity)
    {
        entity.SchedulerInstanceId = entry.SchedulerInstanceId;
        entity.SchedulerName = entry.SchedulerName;
        entity.Job = entry.Job;
        entity.Trigger = entry.Trigger;
        entity.ScheduledFireTimeUtc = entry.ScheduledFireTimeUtc;
        entity.ActualFireTimeUtc = entry.ActualFireTimeUtc;
        entity.Recovering = entry.Recovering;
        entity.Vetoed = entry.Vetoed;
        entity.FinishedTimeUtc = entry.FinishedTimeUtc;
        entity.ExceptionMessage = entry.ExceptionMessage;
        return entity;
    }

    public static ExecutionHistoryEntry ToEntry(this QuartzExecutionHistory entity)
    {
        return new ExecutionHistoryEntry()
        {
            FireInstanceId = entity.FireInstanceId,
            SchedulerInstanceId = entity.SchedulerInstanceId,
            SchedulerName = entity.SchedulerName,
            Job = entity.Job,
            Trigger = entity.Trigger,
            ScheduledFireTimeUtc = entity.ScheduledFireTimeUtc,
            ActualFireTimeUtc = entity.ActualFireTimeUtc,
            Recovering = entity.Recovering,
            Vetoed = entity.Vetoed,
            FinishedTimeUtc = entity.FinishedTimeUtc,
            ExceptionMessage = entity.ExceptionMessage,
        };
    }
}
