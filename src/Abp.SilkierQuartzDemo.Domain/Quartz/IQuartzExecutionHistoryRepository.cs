using System;
using System.Threading.Tasks;
using System.Threading;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;

namespace Abp.SilkierQuartzDemo.Quartz;

public interface IQuartzExecutionHistoryRepository : IBasicRepository<QuartzExecutionHistory, Guid>
{
    Task<QuartzExecutionHistory?> FindByFireInstanceIdAsync(string fireInstanceId, CancellationToken cancellationToken = default);

    Task<List<QuartzExecutionHistory>> GetLastOfEveryJobAsync(string schedulerName, int limitPerJob, CancellationToken cancellationToken = default);

    Task<List<QuartzExecutionHistory>> GetLastOfEveryTriggerAsync(
        string schedulerName,
        int limitPerTrigger,
        int skipPerTrigger = 0,
        CancellationToken cancellationToken = default);

    Task<List<QuartzExecutionHistory>> GetLastAsync(string schedulerName, int limit, CancellationToken cancellationToken = default);

    Task PurgeAsync(CancellationToken cancellationToken = default);
}
