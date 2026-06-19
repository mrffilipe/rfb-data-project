using RFBDataProject.Application.Services.UnitOfWork;
using RFBDataProject.Infrastructure.Caching;
using RFBDataProject.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace RFBDataProject.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<SearchCacheService>();

        return services;
    }
}
