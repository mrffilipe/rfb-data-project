namespace RFBDataProject.Application.Services.Rfb;

public interface IRfbDiscoveryService
{
    Task<RfbDiscoveryResult?> DiscoverLatestAsync(CancellationToken ct = default);
}
