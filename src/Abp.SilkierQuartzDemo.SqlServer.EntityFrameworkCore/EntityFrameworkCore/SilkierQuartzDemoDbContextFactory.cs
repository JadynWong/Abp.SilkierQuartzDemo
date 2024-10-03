using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Abp.SilkierQuartzDemo.SqlServer.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class SilkierQuartzDemoDbContextFactory : IDesignTimeDbContextFactory<SilkierQuartzDemoDbContext>
{
    public SilkierQuartzDemoDbContext CreateDbContext(string[] args)
    {
        SilkierQuartzDemoEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<SilkierQuartzDemoDbContext>()
                .UseSqlServer(configuration.GetConnectionString("SqlServer"));
            return new SilkierQuartzDemoDbContext(builder.Options);

    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Abp.SilkierQuartzDemo.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
