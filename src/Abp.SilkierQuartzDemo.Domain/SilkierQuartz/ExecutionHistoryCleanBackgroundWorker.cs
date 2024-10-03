using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace Abp.SilkierQuartzDemo.SilkierQuartz;

public class ExecutionHistoryCleanBackgroundWorker : AsyncPeriodicBackgroundWorkerBase, ITransientDependency
{
    public ExecutionHistoryCleanBackgroundWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 1000 * 60; //1 minutes
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogDebug("ExecutionHistoryCleanBackgroundWorker executing");

        await LazyServiceProvider
            .LazyGetRequiredService<AbpExecutionHistoryStore>()
            .Purge();

        Logger.LogDebug("ExecutionHistoryCleanBackgroundWorker executed");
    }
}
