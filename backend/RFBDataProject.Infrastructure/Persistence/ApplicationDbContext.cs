using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace RFBDataProject.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<IngestionRelease> IngestionReleases => Set<IngestionRelease>();
    public DbSet<IngestionArtifact> IngestionArtifacts => Set<IngestionArtifact>();
    public DbSet<IngestionRun> IngestionRuns => Set<IngestionRun>();
    public DbSet<ImportExecution> ImportExecutions => Set<ImportExecution>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreatedAt();
            else if (entry.State == EntityState.Modified)
                entry.Entity.SetUpdatedAt();
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
