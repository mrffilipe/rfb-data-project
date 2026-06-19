using RFBDataProject.Infrastructure.Configurations;
using RFBDataProject.Infrastructure.Persistence;
using RFBDataProject.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RFBDataProject.Infrastructure.Exceptions;

namespace RFBDataProject.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SECTION))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SECTION))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RedisOptions>, RedisOptionsValidator>();

        services.AddOptions<RfbOptions>()
            .Bind(configuration.GetSection(RfbOptions.SECTION))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RfbOptions>, RfbOptionsValidator>();

        var redisOptions = configuration.GetSection(RedisOptions.SECTION).Get<RedisOptions>() ?? new RedisOptions();
        if (!string.IsNullOrWhiteSpace(redisOptions.ConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = redisOptions.InstanceName;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        var connectionString = configuration.GetSection(DatabaseOptions.SECTION).Get<DatabaseOptions>()?.ConnectionString
            ?? throw new InvalidOperationException(InfrastructureErrorMessages.Database.CONNECTION_STRING_REQUIRED);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpClient("Rfb");

        services.AddRepositories();
        services.AddAggregateServices();
        services.AddServices();
        services.AddHostedService<IngestionBackgroundService>();

        return services;
    }
}
