using Volo.Abp.Settings;

namespace Abp.SilkierQuartzDemo.Settings;

public class SilkierQuartzDemoSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(SilkierQuartzDemoSettings.MySetting1));
    }
}
