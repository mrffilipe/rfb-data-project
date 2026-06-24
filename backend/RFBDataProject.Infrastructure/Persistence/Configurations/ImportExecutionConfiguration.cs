using RFBDataProject.Domain.Entities;
using RFBDataProject.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RFBDataProject.Infrastructure.Persistence.Configurations;

public sealed class ImportExecutionConfiguration : BaseEntityConfiguration<ImportExecution>
{
    public override void Configure(EntityTypeBuilder<ImportExecution> builder)
    {
        base.Configure(builder);

        builder.ToTable("import_executions");

        builder.Property(x => x.RunId)
            .HasColumnName("run_id")
            .IsRequired();

        builder.Property(x => x.ReferencePeriod)
            .HasColumnName("referencia_receita")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasColumnName("data_inicio")
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnName("data_fim");

        builder.Property(x => x.ProcessedCount)
            .HasColumnName("quantidade_processada")
            .IsRequired();

        builder.Property(x => x.InsertedCount)
            .HasColumnName("quantidade_inserida")
            .IsRequired();

        builder.Property(x => x.UpdatedCount)
            .HasColumnName("quantidade_atualizada")
            .IsRequired();

        builder.Property(x => x.IgnoredCount)
            .HasColumnName("quantidade_ignorada")
            .IsRequired();

        builder.Property(x => x.Errors)
            .HasColumnName("erros")
            .HasMaxLength(4000);

        builder.HasIndex(x => x.RunId)
            .IsUnique();

        builder.HasIndex(x => x.StartedAt);
    }
}
