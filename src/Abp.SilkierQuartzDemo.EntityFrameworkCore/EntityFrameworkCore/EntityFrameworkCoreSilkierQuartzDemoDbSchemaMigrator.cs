using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Abp.SilkierQuartzDemo.Data;
using Volo.Abp.DependencyInjection;

namespace Abp.SilkierQuartzDemo.EntityFrameworkCore;

public class EntityFrameworkCoreSilkierQuartzDemoDbSchemaMigrator
    : ISilkierQuartzDemoDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreSilkierQuartzDemoDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the SilkierQuartzDemoDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<SilkierQuartzDemoDbContext>()
            .Database
            .MigrateAsync();
    }
}
