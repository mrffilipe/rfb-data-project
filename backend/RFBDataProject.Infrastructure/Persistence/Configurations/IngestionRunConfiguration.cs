using RFBDataProject.Domain.Entities;
using RFBDataProject.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RFBDataProject.Infrastructure.Persistence.Configurations;

public sealed class IngestionRunConfiguration : BaseEntityConfiguration<IngestionRun>
{
    public override void Configure(EntityTypeBuilder<IngestionRun> builder)
    {
        base.Configure(builder);

        builder.ToTable("ingestion_runs");

        builder.Property(x => x.ReleaseId)
            .HasColumnName("release_id")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(x => x.ArtifactsProcessed)
            .HasColumnName("artifacts_processed")
            .IsRequired();

        builder.Property(x => x.ArtifactsFailed)
            .HasColumnName("artifacts_failed")
            .IsRequired();

        builder.HasIndex(x => x.StartedAt);
    }
}
