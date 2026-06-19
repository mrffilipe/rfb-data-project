namespace RFBDataProject.Infrastructure.Configurations;

public sealed record RedisOptions
{
    public const string SECTION = "Redis";
    public string ConnectionString { get; init; } = string.Empty;
    public string InstanceName { get; init; } = "rfb:";
    public int SearchCacheMinutes { get; init; } = 5;
}
