using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace Abp.SilkierQuartzDemo.Web;

[Dependency(ReplaceServices = true)]
public class SilkierQuartzDemoBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SilkierQuartzDemo";
}
