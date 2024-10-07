using Abp.SilkierQuartzDemo.EntityTypeConfigurations;
using Abp.SilkierQuartzDemo.SilkierQuartz;
using Microsoft.EntityFrameworkCore;
using System;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Abp.SilkierQuartzDemo.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class SilkierQuartzDemoDbContext :
    AbpDbContext<SilkierQuartzDemoDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public SilkierQuartzDemoDbContext(DbContextOptions<SilkierQuartzDemoDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(SilkierQuartzDemoConsts.DbTablePrefix + "YourEntities", SilkierQuartzDemoConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
        
        #region SilkierQuartz
        var prefix = "Quartz";
        var schema = "silkier";

        builder.ApplyConfiguration(
          new SilkierQuartzExecutionHistoryEntityTypeConfiguration(prefix, schema));

        builder.ApplyConfiguration(
          new SilkierQuartzJobSummaryEntityTypeConfiguration(prefix, schema));
        #endregion
        
        #region Quartz
        var quartzPrefix = "qrtz_";
        var quartzSchema = "quartz";

        builder.ApplyConfiguration(
          new QuartzJobDetailEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzTriggerEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzSimpleTriggerEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzSimplePropertyTriggerEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzCronTriggerEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzBlobTriggerEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzCalendarEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzPausedTriggerGroupEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzFiredTriggerEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzSchedulerStateEntityTypeConfiguration(quartzPrefix, quartzSchema));

        builder.ApplyConfiguration(
          new QuartzLockEntityTypeConfiguration(quartzPrefix, quartzSchema));
        #endregion
    }
}
