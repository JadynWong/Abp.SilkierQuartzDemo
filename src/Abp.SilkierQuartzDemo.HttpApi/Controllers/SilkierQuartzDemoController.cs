using Abp.SilkierQuartzDemo.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Abp.SilkierQuartzDemo.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SilkierQuartzDemoController : AbpControllerBase
{
    protected SilkierQuartzDemoController()
    {
        LocalizationResource = typeof(SilkierQuartzDemoResource);
    }
}
