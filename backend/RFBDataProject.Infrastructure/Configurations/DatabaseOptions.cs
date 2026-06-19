namespace RFBDataProject.Infrastructure.Configurations;

public sealed record DatabaseOptions
{
    public const string SECTION = "Database";
    public required string ConnectionString { get; init; }
}
