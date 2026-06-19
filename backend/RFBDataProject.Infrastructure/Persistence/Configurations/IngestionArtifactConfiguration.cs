using RFBDataProject.Domain.Entities;
using RFBDataProject.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RFBDataProject.Infrastructure.Persistence.Configurations;

public sealed class IngestionArtifactConfiguration : BaseEntityConfiguration<IngestionArtifact>
{
    public override void Configure(EntityTypeBuilder<IngestionArtifact> builder)
    {
        base.Configure(builder);

        builder.ToTable("ingestion_artifacts");

        builder.Property(x => x.ReleaseId)
            .HasColumnName("release_id")
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.TargetTable)
            .HasColumnName("target_table")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.RemoteSize)
            .HasColumnName("remote_size");

        builder.Property(x => x.Sha256)
            .HasColumnName("sha256")
            .HasMaxLength(64);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.LoadedAt)
            .HasColumnName("loaded_at");

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.ReleaseId, x.FileName })
            .IsUnique();

        builder.HasIndex(x => x.Status);
    }
}
