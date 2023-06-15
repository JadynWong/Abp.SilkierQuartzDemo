using Abp.SilkierQuartzDemo.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Abp.SilkierQuartzDemo.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class SilkierQuartzDemoPageModel : AbpPageModel
{
    protected SilkierQuartzDemoPageModel()
    {
        LocalizationResourceType = typeof(SilkierQuartzDemoResource);
    }
}
