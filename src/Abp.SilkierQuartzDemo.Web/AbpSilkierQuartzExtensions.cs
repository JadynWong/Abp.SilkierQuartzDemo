using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Quartz;
using System;
using Volo.Abp.Ui.Branding;

namespace Abp.SilkierQuartzDemo.Web;

public static class AbpSilkierQuartzExtensions
{
    private const string DefaultVirtualPathRoot = "/quartz";

    public static IApplicationBuilder UseAbpSilkierQuartz(
        this IApplicationBuilder app,
        Action<SilkierQuartz.SilkierQuartzOptions>? actionOptions = null)
    {
        var options = new SilkierQuartz.SilkierQuartzOptions()
        {
            Logo = app.ApplicationServices.GetRequiredService<IBrandingProvider>().LogoUrl ?? "Content/Images/logo.png",
            Scheduler = app.ApplicationServices.GetRequiredService<IScheduler>(),
            VirtualPathRoot = DefaultVirtualPathRoot,
            UseLocalTime = true,
            DefaultDateFormat = "yyyy-MM-dd",
            DefaultTimeFormat = "HH:mm:ss",
            CronExpressionOptions = new CronExpressionDescriptor.Options()
            {
                DayOfWeekStartIndexZero = false //Quartz uses 1-7 as the range
            }
        };

        actionOptions?.Invoke(options);

        var fsOptions = new FileServerOptions()
        {
            RequestPath = new PathString($"{options.VirtualPathRoot}/Content"),
            EnableDefaultFiles = false,
            EnableDirectoryBrowsing = false,
            FileProvider = new EmbeddedFileProvider(typeof(SilkierQuartz.SilkierQuartzOptions).Assembly, "SilkierQuartz.Content")
        };

        app.UseFileServer(fsOptions);

        var services = SilkierQuartz.Services.Create(options, null);

        app.Use(async (context, next) =>
        {
            context.Items[typeof(SilkierQuartz.Services)] = services;
            await next.Invoke();
        });

        return app;
    }

    public static void MapAbpSilkierQuartz(this IEndpointRouteBuilder routeBuilder, string virtualPathRoot = DefaultVirtualPathRoot)
    {
        routeBuilder.MapControllerRoute(nameof(SilkierQuartz), $"{virtualPathRoot}/{{controller=Scheduler}}/{{action=Index}}");
    }
}
