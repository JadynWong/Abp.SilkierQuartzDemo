using System;
using Volo.Abp.Domain.Entities;

namespace Abp.SilkierQuartzDemo.Quartz;

public class QuartzExecutionHistory : BasicAggregateRoot<Guid>
{
    public string FireInstanceId { get; protected set; } = null!;

    public string SchedulerInstanceId { get; set; } = null!;

    public string SchedulerName { get; set; } = null!;

    public string? Job { get; set; }

    public string? Trigger { get; set; }

    public DateTime? ScheduledFireTimeUtc { get; set; }

    public DateTime ActualFireTimeUtc { get; set; }

    public bool Recovering { get; set; }

    public bool Vetoed { get; set; }

    public DateTime? FinishedTimeUtc { get; set; }

    public string? ExceptionMessage { get; set; }

    protected QuartzExecutionHistory()
    {

    }

    public QuartzExecutionHistory(Guid id, string fireInstanceId) : base(id)
    {
        FireInstanceId = fireInstanceId;
    }
}
