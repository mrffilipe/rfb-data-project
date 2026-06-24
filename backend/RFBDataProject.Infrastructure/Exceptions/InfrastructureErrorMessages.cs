namespace RFBDataProject.Infrastructure.Exceptions;

public static class InfrastructureErrorMessages
{
    public static class Database
    {
        public const string CONNECTION_STRING_REQUIRED = "Database connection string is required.";
    }

    public static class Rfb
    {
        public const string BASE_URLS_REQUIRED = "At least one RFB base URL is required.";
        public const string INVALID_PARALLELISM = "Download parallelism must be between 1 and 16.";
        public const string INVALID_PARSER_PARALLELISM = "Parser parallelism must be between 1 and 16.";
        public const string INVALID_STAGING_PARALLELISM = "Staging writer parallelism must be between 1 and 16.";
        public const string INVALID_TRANSFORM_PARALLELISM = "Transform parallelism must be between 1 and 16.";
        public const string INVALID_UPSERT_PARALLELISM = "Final upsert parallelism must be between 1 and 16.";
        public const string INVALID_CHANNEL_CAPACITY = "Channel capacity must be between 100 and 1000000.";
        public const string INVALID_STAGING_BATCH = "Staging batch size must be between 100 and 100000.";
        public const string INVALID_UPSERT_BATCH = "Upsert batch size must be between 50 and 50000.";
        public const string INVALID_SYNC_INTERVAL = "Sync interval hours must be at least 1.";
    }

    public static class Redis
    {
        public const string INSTANCE_NAME_REQUIRED = "Redis instance name is required when connection string is set.";
    }
}
