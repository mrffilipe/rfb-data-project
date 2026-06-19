using System.Text.RegularExpressions;
using RFBDataProject.Application.Services.Rfb;
using RFBDataProject.Domain.Constants;
using RFBDataProject.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RFBDataProject.Infrastructure.Rfb;

public sealed partial class RfbDiscoveryService : IRfbDiscoveryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RfbOptions _options;
    private readonly ILogger<RfbDiscoveryService> _logger;

    public RfbDiscoveryService(
        IHttpClientFactory httpClientFactory,
        IOptions<RfbOptions> options,
        ILogger<RfbDiscoveryService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RfbDiscoveryResult?> DiscoverLatestAsync(CancellationToken ct = default)
    {
        foreach (var baseUrl in _options.BaseUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            try
            {
                var referencePeriod = await FindLatestReferencePeriodAsync(baseUrl.TrimEnd('/'), ct);
                if (referencePeriod is null)
                    continue;

                var artifacts = BuildArtifacts(baseUrl.TrimEnd('/'), referencePeriod);
                return new RfbDiscoveryResult
                {
                    ReferencePeriod = referencePeriod,
                    BaseUrl = baseUrl.TrimEnd('/'),
                    Artifacts = artifacts
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover RFB data at {BaseUrl}", baseUrl);
            }
        }

        return null;
    }

    private async Task<string?> FindLatestReferencePeriodAsync(string baseUrl, CancellationToken ct)
    {
        var client = CreateClient();
        var html = await client.GetStringAsync($"{baseUrl}/", ct);
        var matches = ReferencePeriodRegex().Matches(html)
            .Select(m => m.Value)
            .Distinct()
            .OrderDescending()
            .ToList();

        foreach (var match in matches)
        {
            var probe = $"{baseUrl}/{match}/Empresas0.zip";
            using var request = new HttpRequestMessage(HttpMethod.Head, probe);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (response.IsSuccessStatusCode)
                return match;
        }

        for (var i = 0; i < 24; i++)
        {
            var date = DateTime.UtcNow.AddMonths(-i);
            var candidates = new[]
            {
                date.ToString("yyyy-MM-dd"),
                date.ToString("yyyy-MM")
            };

            foreach (var candidate in candidates)
            {
                var probe = $"{baseUrl}/{candidate}/Empresas0.zip";
                using var request = new HttpRequestMessage(HttpMethod.Head, probe);
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                if (response.IsSuccessStatusCode)
                    return candidate;
            }
        }

        return null;
    }

    private static List<RfbRemoteArtifact> BuildArtifacts(string baseUrl, string referencePeriod)
    {
        var artifacts = new List<RfbRemoteArtifact>();
        foreach (var fileName in RfbArtifactDefinitions.ExpectedZipFileNames())
        {
            var targetTable = RfbArtifactDefinitions.All
                .First(d => fileName.StartsWith(d.Prefix, StringComparison.OrdinalIgnoreCase))
                .TargetTable;

            artifacts.Add(new RfbRemoteArtifact
            {
                FileName = fileName,
                TargetTable = targetTable,
                DownloadUrl = $"{baseUrl}/{referencePeriod}/{fileName}"
            });
        }

        return artifacts;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("Rfb");
        client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
        client.Timeout = TimeSpan.FromMinutes(30);
        return client;
    }

    [GeneratedRegex(@"\d{4}-\d{2}(-\d{2})?")]
    private static partial Regex ReferencePeriodRegex();
}
