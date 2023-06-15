using System;
using Volo.Abp.Domain.Entities;

namespace Abp.SilkierQuartzDemo.Quartz;

public class QuartzJobSummary : BasicAggregateRoot<Guid>
{
    public string SchedulerName { get; protected set; } = null!;

    public int TotalJobsExecuted { get; set; }

    public int TotalJobsFailed { get; set; }

    protected QuartzJobSummary()
    {

    }

    public QuartzJobSummary(Guid id, string schedulerName) : base(id)
    {
        SchedulerName = schedulerName;
    }
}
