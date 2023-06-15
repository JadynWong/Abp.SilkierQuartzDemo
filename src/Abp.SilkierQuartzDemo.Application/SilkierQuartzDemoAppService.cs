using System;
using System.Collections.Generic;
using System.Text;
using Abp.SilkierQuartzDemo.Localization;
using Volo.Abp.Application.Services;

namespace Abp.SilkierQuartzDemo;

/* Inherit your application services from this class.
 */
public abstract class SilkierQuartzDemoAppService : ApplicationService
{
    protected SilkierQuartzDemoAppService()
    {
        LocalizationResource = typeof(SilkierQuartzDemoResource);
    }
}
