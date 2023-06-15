using System.Threading.Tasks;

namespace Abp.SilkierQuartzDemo.Data;

public interface ISilkierQuartzDemoDbSchemaMigrator
{
    Task MigrateAsync();
}
