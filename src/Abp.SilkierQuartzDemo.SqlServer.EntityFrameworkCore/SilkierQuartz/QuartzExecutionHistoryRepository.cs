using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.SilkierQuartzDemo.SilkierQuartz;
using Abp.SilkierQuartzDemo.SqlServer.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Abp.SilkierQuartzDemo.SqlServer.SilkierQuartz;

internal class QuartzExecutionHistoryRepository
    : EfCoreRepository<SilkierQuartzDemoDbContext, QuartzExecutionHistory, Guid>, IQuartzExecutionHistoryRepository
{
    public QuartzExecutionHistoryRepository(IDbContextProvider<SilkierQuartzDemoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<QuartzExecutionHistory?> FindByFireInstanceIdAsync(string fireInstanceId, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .FirstOrDefaultAsync(x => x.FireInstanceId == fireInstanceId, GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<QuartzExecutionHistory>> GetLastOfEveryJobAsync(string schedulerName, int limitPerJob, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        var sub = query.OrderByDescending(a => a.ActualFireTimeUtc);

        var quartzJobHistories = await query
            .Where(x => x.SchedulerName == schedulerName)
            .Select(x => x.Job)
            .Distinct()
            .SelectMany(a => sub.Where(b => b.Job == a).Take(limitPerJob), (a, b) => b)
            .ToListAsync(GetCancellationToken(cancellationToken));

        return quartzJobHistories
            .OrderBy(x => x.Trigger)
            .ThenBy(x => x.ActualFireTimeUtc)
            .ToList();
    }

    public virtual async Task<List<QuartzExecutionHistory>> GetLastOfEveryTriggerAsync(
        string schedulerName,
        int limitPerTrigger,
        int skipPerTrigger = 0,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        var sub = query.OrderByDescending(a => a.ActualFireTimeUtc);

        var quartzJobHistories = await query.Where(x => x.SchedulerName == schedulerName)
            .Select(x => x.Trigger)
            .Distinct()
            .SelectMany(a => sub.Where(b => b.Trigger == a).Skip(skipPerTrigger).Take(limitPerTrigger), (a, b) => b)
            .ToListAsync(GetCancellationToken(cancellationToken));

        quartzJobHistories = quartzJobHistories.OrderBy(x => x.Trigger).ThenBy(x => x.ActualFireTimeUtc).ToList();

        return quartzJobHistories;
    }

    public virtual async Task<List<QuartzExecutionHistory>> GetLastAsync(string schedulerName, int limit, CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        var quartzJobHistories = await query
            .Where(x => x.SchedulerName == schedulerName)
            .OrderByDescending(y => y.ActualFireTimeUtc)
            .Take(limit)
            .ToListAsync(GetCancellationToken(cancellationToken));

        quartzJobHistories.Reverse();

        return quartzJobHistories;
    }

    public virtual async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();

        var sub = query.OrderByDescending(a => a.ActualFireTimeUtc);

        query = query
            .OrderByDescending(a => a.ActualFireTimeUtc)
            .Select(x => x.Trigger)
            .Distinct()
            .SelectMany(trigger => sub.Where(b => b.Trigger == trigger).Skip(10), (a, b) => b);

        await query.ExecuteDeleteAsync(GetCancellationToken(cancellationToken));
    }
}
