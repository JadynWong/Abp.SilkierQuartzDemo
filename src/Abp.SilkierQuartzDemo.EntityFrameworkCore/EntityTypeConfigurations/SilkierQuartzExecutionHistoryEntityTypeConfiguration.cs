using Abp.SilkierQuartzDemo.SilkierQuartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Abp.SilkierQuartzDemo.EntityTypeConfigurations
{
    public class SilkierQuartzExecutionHistoryEntityTypeConfiguration : IEntityTypeConfiguration<QuartzExecutionHistory>
    {
        private readonly string? prefix;
        private readonly string? schema;

        public SilkierQuartzExecutionHistoryEntityTypeConfiguration(string? prefix, string? schema)
        {
            this.prefix = prefix;
            this.schema = schema;
        }

        public void Configure(EntityTypeBuilder<QuartzExecutionHistory> b)
        {
            b.ToTable($"{prefix}ExecutionHistories", schema);

            b.ConfigureByConvention();

            // Configure more properties here 
            b.Property(x => x.FireInstanceId)
                .HasMaxLength(200);
            b.Property(x => x.SchedulerInstanceId)
                .HasMaxLength(200);
            b.Property(x => x.SchedulerName)
                .HasMaxLength(200);
            b.Property(x => x.Job)
                .HasMaxLength(300);
            b.Property(x => x.Trigger)
                .HasMaxLength(300);
            b.Property(x => x.ScheduledFireTimeUtc)
                .HasColumnType("timestamp with time zone");
            b.Property(x => x.ActualFireTimeUtc)
                .HasColumnType("timestamp with time zone");
            b.Property(x => x.FinishedTimeUtc)
                .HasColumnType("timestamp with time zone");
                
            b.HasIndex(x => x.FireInstanceId);
        }
    }
}
