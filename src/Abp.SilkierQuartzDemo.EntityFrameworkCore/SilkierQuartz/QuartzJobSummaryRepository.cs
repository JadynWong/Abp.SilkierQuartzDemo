using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.SilkierQuartzDemo.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Abp.SilkierQuartzDemo.SilkierQuartz;

public class QuartzJobSummaryRepository
    : EfCoreRepository<SilkierQuartzDemoDbContext, QuartzJobSummary, Guid>, IQuartzJobSummaryRepository
{
    public QuartzJobSummaryRepository(IDbContextProvider<SilkierQuartzDemoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<QuartzJobSummary?> FindBySchedulerNameAsync(string schedulerName, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SchedulerName == schedulerName, GetCancellationToken(cancellationToken));
    }

    public virtual async Task<int> GetTotalJobsExecutedAsync(string schedulerName, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SchedulerName == schedulerName)
            .Select(x => x.TotalJobsExecuted)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<int> GetTotalJobsFailedAsync(string schedulerName, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SchedulerName == schedulerName)
            .Select(x => x.TotalJobsFailed)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task IncrementTotalJobsExecutedAsync(string schedulerName, CancellationToken cancellationToken = default)
    {
        await (await GetQueryableAsync())
            .Where(x => x.SchedulerName == schedulerName)
            .ExecuteUpdateAsync(
                x => x.SetProperty(qjs => qjs.TotalJobsExecuted, qjs => qjs.TotalJobsExecuted + 1),
                GetCancellationToken(cancellationToken)
            );
    }

    public virtual async Task IncrementTotalJobsFailedAsync(string schedulerName, CancellationToken cancellationToken = default)
    {
        await (await GetQueryableAsync())
            .Where(x => x.SchedulerName == schedulerName)
            .ExecuteUpdateAsync(
                x => x.SetProperty(qjs => qjs.TotalJobsFailed, qjs => qjs.TotalJobsFailed + 1),
                GetCancellationToken(cancellationToken)
            );
    }
}
