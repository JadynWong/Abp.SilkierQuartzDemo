using System;
using System.IO;
using Abp.SilkierQuartzDemo.EntityFrameworkCore;
using Abp.SilkierQuartzDemo.Localization;
using Abp.SilkierQuartzDemo.MultiTenancy;
using Abp.SilkierQuartzDemo.SilkierQuartz;
using Abp.SilkierQuartzDemo.SqlServer.EntityFrameworkCore;
using Abp.SilkierQuartzDemo.Web.Menus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Plugins.RecentHistory;
using Quartz.Util;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundJobs.Quartz;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.Quartz;
using Volo.Abp.SettingManagement.Web;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using static Quartz.SchedulerBuilder;

namespace Abp.SilkierQuartzDemo.Web;

[DependsOn(typeof(SilkierQuartzDemoHttpApiModule))]
[DependsOn(typeof(SilkierQuartzDemoApplicationModule))]
[DependsOn(typeof(SilkierQuartzDemoEntityFrameworkCoreModule))] /* if you need work with postgre remark this line */
//[DependsOn(typeof(SilkierQuartzDemoSqlServerEntityFrameworkCoreModule))] /* if you need work with sqlserver add this line*/
[DependsOn(typeof(AbpAutofacModule))]
[DependsOn(typeof(AbpIdentityWebModule))]
[DependsOn(typeof(AbpSettingManagementWebModule))]
[DependsOn(typeof(AbpAccountWebOpenIddictModule))]
[DependsOn(typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule))]
[DependsOn(typeof(AbpTenantManagementWebModule))]
[DependsOn(typeof(AbpAspNetCoreSerilogModule))]
[DependsOn(typeof(AbpSwashbuckleModule))]
[DependsOn(typeof(AbpBackgroundWorkersQuartzModule))]
[DependsOn(typeof(AbpBackgroundJobsQuartzModule))]
public class SilkierQuartzDemoWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(SilkierQuartzDemoResource),
                typeof(SilkierQuartzDemoDomainModule).Assembly,
                typeof(SilkierQuartzDemoDomainSharedModule).Assembly,
                typeof(SilkierQuartzDemoApplicationModule).Assembly,
                typeof(SilkierQuartzDemoApplicationContractsModule).Assembly,
                typeof(SilkierQuartzDemoWebModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("SilkierQuartzDemo");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        PreConfigure<AbpQuartzOptions>(options =>
        {
            options.Configurator = configure =>
            {
                configure.SetProperty("quartz.plugin.recentHistory.type", typeof(AbpExecutionHistoryPlugin).AssemblyQualifiedNameWithoutVersion());
                configure.SetProperty("quartz.plugin.recentHistory.storeType", typeof(AbpExecutionHistoryStore).AssemblyQualifiedNameWithoutVersion());
                configure.UsePersistentStore(storeOptions =>
                {
                    storeOptions.UseProperties = true;
                    storeOptions.UseNewtonsoftJsonSerializer();
                    //storeOptions.UseSqlServer(configuration.GetConnectionString("SqlServer")!);
                    storeOptions.UsePostgres(configurer =>
                    {
                        configurer.UseDriverDelegate<PostgreSQLDelegate>();;
                        configurer.TablePrefix = "quartz.qrtz_";
                        configurer.ConnectionString = configuration.GetConnectionString("Default")!;
                        //configurer.ConnectionStringName = "Default";
                    });
                    storeOptions.UseClustering(c =>
                    {
                        c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
                        c.CheckinInterval = TimeSpan.FromSeconds(10);
                    });
                });
            };
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);
        Configure<AbpAntiForgeryOptions>(options =>
        {
            options.AutoValidateFilter = type => !type.FullName!.StartsWith("SilkierQuartz.Controllers");
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SilkierQuartzDemoWebModule>();
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<SilkierQuartzDemoDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Abp.SilkierQuartzDemo.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<SilkierQuartzDemoDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Abp.SilkierQuartzDemo.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<SilkierQuartzDemoApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Abp.SilkierQuartzDemo.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<SilkierQuartzDemoApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Abp.SilkierQuartzDemo.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<SilkierQuartzDemoWebModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new SilkierQuartzDemoMenuContributor());
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(SilkierQuartzDemoApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SilkierQuartzDemo API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SilkierQuartzDemo API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseAbpSilkierQuartz();
        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapAbpSilkierQuartz();
        });
    }
}
