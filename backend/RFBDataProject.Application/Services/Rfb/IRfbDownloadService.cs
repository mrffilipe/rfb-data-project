namespace RFBDataProject.Application.Services.Rfb;

public interface IRfbDownloadService
{
    Task<Stream> DownloadAsStreamAsync(string url, CancellationToken ct = default);
    Task<long?> GetRemoteSizeAsync(string url, CancellationToken ct = default);
}
