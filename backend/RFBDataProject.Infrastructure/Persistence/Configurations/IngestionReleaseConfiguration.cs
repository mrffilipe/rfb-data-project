using RFBDataProject.Domain.Entities;
using RFBDataProject.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RFBDataProject.Infrastructure.Persistence.Configurations;

public sealed class IngestionReleaseConfiguration : BaseEntityConfiguration<IngestionRelease>
{
    public override void Configure(EntityTypeBuilder<IngestionRelease> builder)
    {
        base.Configure(builder);

        builder.ToTable("ingestion_releases");

        builder.Property(x => x.ReferencePeriod)
            .HasColumnName("competencia")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.BaseUrl)
            .HasColumnName("base_url")
            .HasMaxLength(500);

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at");

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.HasIndex(x => x.ReferencePeriod)
            .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.Artifacts)
            .WithOne()
            .HasForeignKey(x => x.ReleaseId);
    }
}
