using Microsoft.Extensions.Caching.Memory;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// In-process <see cref="IMemoryCache"/>-backed implementation of <see cref="ICacheService"/>.
/// </summary>
/// <remarks>
/// <para>
/// Phase 1 uses <see cref="IMemoryCache"/> only. When distributed caching (Redis) is
/// required in a future phase, this class can be replaced with a Redis-backed
/// implementation without any Application-layer changes.
/// </para>
/// <para>
/// Callers are responsible for including the <c>TenantId</c> or <c>UserId</c> in the
/// cache key to prevent cross-tenant cache leakage (per <c>SECURITY_GUIDELINES.md</c>).
/// </para>
/// </remarks>
internal sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Default cache entry TTL applied when the caller does not specify an expiration.
    /// </summary>
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initialises a new <see cref="MemoryCacheService"/>.
    /// </summary>
    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc/>
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    /// <inheritdoc/>
    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
        };

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }
}
