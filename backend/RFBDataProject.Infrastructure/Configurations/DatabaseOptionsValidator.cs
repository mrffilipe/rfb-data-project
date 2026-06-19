using Microsoft.Extensions.Options;
using RFBDataProject.Infrastructure.Exceptions;

namespace RFBDataProject.Infrastructure.Configurations;

public sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            return ValidateOptionsResult.Fail(InfrastructureErrorMessages.Database.CONNECTION_STRING_REQUIRED);

        return ValidateOptionsResult.Success;
    }
}
