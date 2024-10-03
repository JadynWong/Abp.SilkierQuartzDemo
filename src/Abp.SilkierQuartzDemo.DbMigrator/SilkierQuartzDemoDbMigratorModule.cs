using Abp.SilkierQuartzDemo.EntityFrameworkCore;
using Abp.SilkierQuartzDemo.SqlServer.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Abp.SilkierQuartzDemo.DbMigrator;

[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(SilkierQuartzDemoEntityFrameworkCoreModule))] /* if you need work with postgre remark this line */
//[DependsOn(typeof(SilkierQuartzDemoSqlServerEntityFrameworkCoreModule))] /* if you need work with sqlserver add this line*/
[DependsOn(typeof(SilkierQuartzDemoApplicationContractsModule))]
public class SilkierQuartzDemoDbMigratorModule : AbpModule
{

}
