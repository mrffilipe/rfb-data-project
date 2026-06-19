using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace RFBDataProject.Infrastructure.Caching;

public sealed class SearchCacheService
{
    private readonly IDistributedCache _cache;
    private readonly Configurations.RedisOptions _options;

    public SearchCacheService(IDistributedCache cache, Microsoft.Extensions.Options.IOptions<Configurations.RedisOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public async Task<T?> GetAsync<T>(string keyPrefix, object request, CancellationToken ct = default)
    {
        var key = BuildKey(keyPrefix, request);
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes is null || bytes.Length == 0)
            return default;

        return JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task SetAsync<T>(string keyPrefix, object request, T value, CancellationToken ct = default)
    {
        var key = BuildKey(keyPrefix, request);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        await _cache.SetAsync(
            key,
            bytes,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.SearchCacheMinutes)
            },
            ct);
    }

    private static string BuildKey(string prefix, object request)
    {
        var json = JsonSerializer.Serialize(request);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        return $"{prefix}{hash}";
    }
}
