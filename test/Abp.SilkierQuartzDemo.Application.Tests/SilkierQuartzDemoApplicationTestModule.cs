using Volo.Abp.Modularity;

namespace Abp.SilkierQuartzDemo;

[DependsOn(
    typeof(SilkierQuartzDemoApplicationModule),
    typeof(SilkierQuartzDemoDomainTestModule)
    )]
public class SilkierQuartzDemoApplicationTestModule : AbpModule
{

}
