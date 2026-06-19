using RFBDataProject.Application.Services.Rfb;
using RFBDataProject.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RFBDataProject.Infrastructure.Rfb;

public sealed class RfbDownloadService : IRfbDownloadService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RfbOptions _options;
    private readonly ILogger<RfbDownloadService> _logger;

    public RfbDownloadService(
        IHttpClientFactory httpClientFactory,
        IOptions<RfbOptions> options,
        ILogger<RfbDownloadService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<long?> GetRemoteSizeAsync(string url, CancellationToken ct = default)
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return response.Content.Headers.ContentLength;
    }

    public async Task<Stream> DownloadToTempFileStreamAsync(string url, CancellationToken ct = default)
    {
        var client = CreateClient();
        var tempPath = Path.Combine(Path.GetTempPath(), $"rfb_{Guid.NewGuid():N}.zip");

        for (var attempt = 1; attempt <= _options.MaxDownloadRetries; attempt++)
        {
            try
            {
                await DownloadWithResumeAsync(client, url, tempPath, ct);
                return new TempFileStream(tempPath);
            }
            catch (Exception ex) when (attempt < _options.MaxDownloadRetries)
            {
                _logger.LogWarning(ex, "Download attempt {Attempt} failed for {Url}", attempt, url);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
            }
        }

        await DownloadWithResumeAsync(client, url, tempPath, ct);
        return new TempFileStream(tempPath);
    }

    private static async Task DownloadWithResumeAsync(HttpClient client, string url, string tempPath, CancellationToken ct)
    {
        long offset = 0;
        if (File.Exists(tempPath))
            offset = new FileInfo(tempPath).Length;

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (offset > 0)
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(offset, null);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var network = await response.Content.ReadAsStreamAsync(ct);
        await using var file = new FileStream(
            tempPath,
            offset > 0 ? FileMode.Append : FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            1024 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        await network.CopyToAsync(file, ct);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("Rfb");
        client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
        client.Timeout = TimeSpan.FromMinutes(60);
        return client;
    }

    private sealed class TempFileStream : FileStream
    {
        private readonly string _path;

        public TempFileStream(string path)
            : base(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.DeleteOnClose | FileOptions.Asynchronous)
        {
            _path = path;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try
            {
                if (File.Exists(_path))
                    File.Delete(_path);
            }
            catch
            {
                // Best effort cleanup.
            }
        }
    }
}
