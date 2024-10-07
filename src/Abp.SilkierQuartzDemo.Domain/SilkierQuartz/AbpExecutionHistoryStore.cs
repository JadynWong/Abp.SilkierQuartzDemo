using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz.Plugins.RecentHistory;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace Abp.SilkierQuartzDemo.SilkierQuartz;

public class AbpExecutionHistoryStore : IExecutionHistoryStore, ISingletonDependency
{
    public string SchedulerName { get; set; } = null!;

    protected IServiceScopeFactory ServiceScopeFactory = null!;

    public ILogger<AbpExecutionHistoryStore> Logger { get; set; }

    public AbpExecutionHistoryStore()
    {
        Logger = NullLogger<AbpExecutionHistoryStore>.Instance;
    }

    public AbpExecutionHistoryStore(
        IServiceScopeFactory serviceScopeFactory) : this()
    {
        ServiceScopeFactory = serviceScopeFactory;
    }

    public async Task<ExecutionHistoryEntry?> Get(string fireInstanceId)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzExecutionHistoryRepository>();

        var quartzJobHistory = await repository.FindByFireInstanceIdAsync(fireInstanceId);
        if (quartzJobHistory == null)
        {
            return null;
        }
        return quartzJobHistory.ToEntry();
    }

    public async Task Purge()
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzExecutionHistoryRepository>();

        await repository.PurgeAsync();
    }

    public async Task Save(ExecutionHistoryEntry entry)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var guidGenerator = scope.ServiceProvider.GetRequiredService<IGuidGenerator>();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzExecutionHistoryRepository>();

        var quartzJobHistory = await repository.FindByFireInstanceIdAsync(entry.FireInstanceId);
        if (quartzJobHistory == null)
        {
            quartzJobHistory = entry.ToEntity(new QuartzExecutionHistory(guidGenerator.Create(), entry.FireInstanceId));
            await repository.InsertAsync(quartzJobHistory);
        }
        else
        {
            quartzJobHistory = entry.ToEntity(quartzJobHistory);
            await repository.UpdateAsync(quartzJobHistory);
        }
    }

    public async Task<IEnumerable<ExecutionHistoryEntry>> FilterLastOfEveryJob(int limitPerJob)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzExecutionHistoryRepository>();

        var quartzJobHistories = await repository.GetLastOfEveryJobAsync(SchedulerName, limitPerJob);

        return quartzJobHistories.Select(x => x.ToEntry());
    }

    public async Task<IEnumerable<ExecutionHistoryEntry>> FilterLastOfEveryTrigger(int limitPerTrigger)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzExecutionHistoryRepository>();

        var quartzJobHistories = await repository.GetLastOfEveryTriggerAsync(SchedulerName, limitPerTrigger);

        return quartzJobHistories.Select(x => x.ToEntry());
    }

    public async Task<IEnumerable<ExecutionHistoryEntry>> FilterLast(int limit)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzExecutionHistoryRepository>();

        var quartzJobHistories = await repository.GetLastAsync(SchedulerName, limit);

        return quartzJobHistories.Select(x => x.ToEntry());
    }

    public async Task<int> GetTotalJobsExecuted()
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzJobSummaryRepository>();

        return await repository.GetTotalJobsExecutedAsync(SchedulerName);
    }

    public async Task<int> GetTotalJobsFailed()
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzJobSummaryRepository>();

        return await repository.GetTotalJobsFailedAsync(SchedulerName);
    }

    public async Task IncrementTotalJobsExecuted()
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzJobSummaryRepository>();

        await repository.IncrementTotalJobsExecutedAsync(SchedulerName);
    }

    public async Task IncrementTotalJobsFailed()
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzJobSummaryRepository>();

        await repository.IncrementTotalJobsFailedAsync(SchedulerName);
    }

    public virtual async Task InitializeSummaryAsync()
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var repository = scope.ServiceProvider.GetRequiredService<IQuartzJobSummaryRepository>();
        var guidGenerator = scope.ServiceProvider.GetRequiredService<IGuidGenerator>();

        using var uow = unitOfWorkManager.Begin(true);

        var quartzJobSummary = await repository.FindBySchedulerNameAsync(SchedulerName);
        if (quartzJobSummary == null)
        {
            quartzJobSummary = new QuartzJobSummary(guidGenerator.Create(), SchedulerName);
            await repository.InsertAsync(quartzJobSummary);
        }

        await uow.CompleteAsync();
    }
}