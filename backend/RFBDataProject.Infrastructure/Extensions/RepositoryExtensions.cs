using RFBDataProject.Domain.Repositories;
using RFBDataProject.Infrastructure.Ingestion.Metrics;
using RFBDataProject.Infrastructure.Ingestion.Persistence;
using RFBDataProject.Infrastructure.Ingestion.Pipeline;
using RFBDataProject.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace RFBDataProject.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IIngestionReleaseRepository, IngestionReleaseRepository>();
        services.AddScoped<IIngestionRunRepository, IngestionRunRepository>();
        services.AddScoped<IImportExecutionRepository, ImportExecutionRepository>();
        services.AddScoped<ICnpjBulkRepository, CnpjBulkRepository>();
        services.AddSingleton<PipelineChannelFactory>();
        services.AddSingleton<RfbIngestionMetrics>();

        services.AddScoped<IStagingBulkWriter, StagingBulkWriter>();
        services.AddScoped<INpgsqlBulkConnectionFactory, NpgsqlBulkConnectionFactory>();
        services.AddScoped<IIngestionPipelineOrchestrator, IngestionPipelineOrchestrator>();

        return services;
    }
}
