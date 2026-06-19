using RFBDataProject.Domain.Repositories;
using RFBDataProject.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace RFBDataProject.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IIngestionReleaseRepository, IngestionReleaseRepository>();
        services.AddScoped<IIngestionRunRepository, IngestionRunRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IEstablishmentRepository, EstablishmentRepository>();
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<ICnpjBulkRepository, CnpjBulkRepository>();
        services.AddScoped<ICnpjBulkLoader, Ingestion.CnpjBulkCopyService>();

        return services;
    }
}
