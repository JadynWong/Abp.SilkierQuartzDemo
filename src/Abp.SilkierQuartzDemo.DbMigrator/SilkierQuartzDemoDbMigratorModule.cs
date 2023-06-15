using Abp.SilkierQuartzDemo.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Abp.SilkierQuartzDemo.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SilkierQuartzDemoEntityFrameworkCoreModule),
    typeof(SilkierQuartzDemoApplicationContractsModule)
    )]
public class SilkierQuartzDemoDbMigratorModule : AbpModule
{

}
