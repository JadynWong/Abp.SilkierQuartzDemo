using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Abp.SilkierQuartzDemo.SilkierQuartz;

public interface IQuartzJobSummaryRepository : IBasicRepository<QuartzJobSummary, Guid>
{
    Task<QuartzJobSummary?> FindBySchedulerNameAsync(string schedulerName, CancellationToken cancellationToken = default);

    Task<int> GetTotalJobsExecutedAsync(string schedulerName, CancellationToken cancellationToken = default);

    Task<int> GetTotalJobsFailedAsync(string schedulerName, CancellationToken cancellationToken = default);

    Task IncrementTotalJobsExecutedAsync(string schedulerName, CancellationToken cancellationToken = default);

    Task IncrementTotalJobsFailedAsync(string schedulerName, CancellationToken cancellationToken = default);
}