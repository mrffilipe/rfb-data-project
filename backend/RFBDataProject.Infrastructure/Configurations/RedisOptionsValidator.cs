using Microsoft.Extensions.Options;
using RFBDataProject.Infrastructure.Exceptions;

namespace RFBDataProject.Infrastructure.Configurations;

public sealed class RedisOptionsValidator : IValidateOptions<RedisOptions>
{
    public ValidateOptionsResult Validate(string? name, RedisOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ConnectionString) &&
            string.IsNullOrWhiteSpace(options.InstanceName))
        {
            return ValidateOptionsResult.Fail(InfrastructureErrorMessages.Redis.INSTANCE_NAME_REQUIRED);
        }

        return ValidateOptionsResult.Success;
    }
}
