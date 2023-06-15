using Abp.SilkierQuartzDemo.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Abp.SilkierQuartzDemo;

[DependsOn(
    typeof(SilkierQuartzDemoEntityFrameworkCoreTestModule)
    )]
public class SilkierQuartzDemoDomainTestModule : AbpModule
{

}
