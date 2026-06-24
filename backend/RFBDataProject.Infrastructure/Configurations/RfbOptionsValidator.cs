using Microsoft.Extensions.Options;
using RFBDataProject.Infrastructure.Exceptions;

namespace RFBDataProject.Infrastructure.Configurations;

public sealed class RfbOptionsValidator : IValidateOptions<RfbOptions>
{
    public ValidateOptionsResult Validate(string? name, RfbOptions options)
    {
        if (options.BaseUrls is null || options.BaseUrls.Count == 0 || options.BaseUrls.All(string.IsNullOrWhiteSpace))
            return ValidateOptionsResult.Fail(InfrastructureErrorMessages.Rfb.BASE_URLS_REQUIRED);

        if (options.DownloadParallelism is < 1 or > 16)
            return ValidateOptionsResult.Fail(InfrastructureErrorMessages.Rfb.INVALID_PARALLELISM);

        if (options.ParserParallelism is < 1 or > 16)
            return ValidateOptionsResult.Fail(InfrastructureErrorMessages.Rfb.INVALID_PARSER_PARALLELISM);

        if (options.StagingWriterParallelism is < 1 or > 16)
            return ValidateOptionsResult.Fail(InfrastructureErrorMessages.Rfb.INVALID_STAGING_PARALLELISM);

        if (options.ChannelCapacity is < 100 or > 1_000_000)
            return ValidateOptionsResult.Fail(InfrastructureErrorMessages.Rfb.INVALID_CHANNEL_CAPACITY);

        if (options.StagingBatchSize is < 100 or > 100_000)
            return ValidateOptionsResult.Fail(InfrastructureErrorMessages.Rfb.INVALID_STAGING_BATCH);

        if (options.SyncIntervalHours < 1)
            return ValidateOptionsResult.Fail(InfrastructureErrorMessages.Rfb.INVALID_SYNC_INTERVAL);

        return ValidateOptionsResult.Success;
    }
}
