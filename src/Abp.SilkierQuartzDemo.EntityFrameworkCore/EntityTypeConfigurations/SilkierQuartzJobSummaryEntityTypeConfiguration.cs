using Abp.SilkierQuartzDemo.SilkierQuartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Abp.SilkierQuartzDemo.EntityTypeConfigurations
{
    public class SilkierQuartzJobSummaryEntityTypeConfiguration : IEntityTypeConfiguration<QuartzJobSummary>
    {
        private readonly string? prefix;
        private readonly string? schema;

        public SilkierQuartzJobSummaryEntityTypeConfiguration(string? prefix, string? schema)
        {
            this.prefix = prefix;
            this.schema = schema;
        }

        public void Configure(EntityTypeBuilder<QuartzJobSummary> b)
        {
            b.ToTable($"{prefix}JobSummaries", schema);

            b.ConfigureByConvention();

            // Configure more properties here
            b.Property(x => x.SchedulerName).HasMaxLength(200);
        }
    }
}
