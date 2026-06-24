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

    public async Task<Stream> DownloadAsStreamAsync(string url, CancellationToken ct = default)
    {
        var client = CreateClient();

        for (var attempt = 1; attempt <= _options.MaxDownloadRetries; attempt++)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();
                var network = await response.Content.ReadAsStreamAsync(ct);
                return new HttpResponseStream(network, response);
            }
            catch (Exception ex) when (attempt < _options.MaxDownloadRetries)
            {
                _logger.LogWarning(ex, "Download attempt {Attempt} failed for {Url}", attempt, url);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
            }
        }

        var finalRequest = new HttpRequestMessage(HttpMethod.Get, url);
        var finalResponse = await client.SendAsync(finalRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        finalResponse.EnsureSuccessStatusCode();
        var finalStream = await finalResponse.Content.ReadAsStreamAsync(ct);
        return new HttpResponseStream(finalStream, finalResponse);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("Rfb");
        client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
        client.Timeout = TimeSpan.FromMinutes(60);
        return client;
    }

    private sealed class HttpResponseStream : Stream
    {
        private readonly Stream _inner;
        private readonly HttpResponseMessage _response;
        private bool _disposed;

        public HttpResponseStream(Stream inner, HttpResponseMessage response)
        {
            _inner = inner;
            _response = response;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _inner.Dispose();
                _response.Dispose();
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
