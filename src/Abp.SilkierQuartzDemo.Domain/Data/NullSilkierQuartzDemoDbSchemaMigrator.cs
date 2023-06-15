using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Abp.SilkierQuartzDemo.Data;

/* This is used if database provider does't define
 * ISilkierQuartzDemoDbSchemaMigrator implementation.
 */
public class NullSilkierQuartzDemoDbSchemaMigrator : ISilkierQuartzDemoDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
