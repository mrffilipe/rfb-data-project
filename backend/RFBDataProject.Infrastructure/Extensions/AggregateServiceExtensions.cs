using RFBDataProject.Application.Services.Companies;
using RFBDataProject.Application.Services.Ingestion;
using RFBDataProject.Application.Services.Lookups;
using RFBDataProject.Application.Services.Participations;
using RFBDataProject.Application.Services.Partners;
using RFBDataProject.Application.Services.Rfb;
using RFBDataProject.Infrastructure.Rfb;
using RFBDataProject.Infrastructure.Services.Companies;
using RFBDataProject.Infrastructure.Services.Ingestion;
using RFBDataProject.Infrastructure.Services.Lookups;
using RFBDataProject.Infrastructure.Services.Participations;
using RFBDataProject.Infrastructure.Services.Partners;
using Microsoft.Extensions.DependencyInjection;

namespace RFBDataProject.Infrastructure.Extensions;

public static class AggregateServiceExtensions
{
    public static IServiceCollection AddAggregateServices(this IServiceCollection services)
    {
        services.AddScoped<IIngestionService, IngestionService>();
        services.AddScoped<IRfbDiscoveryService, RfbDiscoveryService>();
        services.AddScoped<IRfbDownloadService, RfbDownloadService>();
        services.AddScoped<ICompanySearchService, CompanySearchService>();
        services.AddScoped<ICompanyQueryService, CompanyQueryService>();
        services.AddScoped<IPartnerQueryService, PartnerQueryService>();
        services.AddScoped<ICorporateParticipationQueryService, CorporateParticipationQueryService>();
        services.AddScoped<ILookupService, LookupService>();

        return services;
    }
}
