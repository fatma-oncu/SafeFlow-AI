namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Provides a technology-agnostic caching abstraction for storing and retrieving
/// serialisable values by string key.
/// </summary>
/// <remarks>
/// <para>
/// The Infrastructure layer may back this interface with any cache provider
/// (Redis, in-memory, SQL-based) without affecting the Application layer.
/// Application-layer query handlers use this interface to cache expensive read
/// results and invalidate them on relevant command completion.
/// </para>
/// <para>
/// All keys are treated as opaque strings.  It is the caller's responsibility to
/// adopt a consistent key naming convention (e.g., <c>"{EntityName}:{Id}"</c>) to
/// prevent collisions across tenants or modules.
/// </para>
/// <para>
/// Implementations must ensure that per-tenant data is scoped by <c>TenantId</c>
/// within the key, or by a tenant-aware key prefix strategy, to prevent cross-tenant
/// cache leakage.
/// </para>
/// </remarks>
public interface ICacheService
{
    /// <summary>
    /// Retrieves the value associated with the specified <paramref name="key"/>, or
    /// <c>null</c> if the key does not exist or has expired.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key. Must not be <c>null</c> or whitespace.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task whose result is the deserialised cached value, or <c>null</c> if the key
    /// is absent or expired.
    /// </returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the specified <paramref name="value"/> under the given <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key. Must not be <c>null</c> or whitespace.</param>
    /// <param name="value">The value to store. Must not be <c>null</c>.</param>
    /// <param name="expiration">
    /// The time-to-live for this cache entry.  When <c>null</c>, the implementation
    /// uses its configured default expiration.
    /// </param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the cache entry associated with the specified <paramref name="key"/>.
    /// A no-op if the key does not exist.
    /// </summary>
    /// <param name="key">The cache key. Must not be <c>null</c> or whitespace.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a non-expired cache entry exists for the specified
    /// <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The cache key. Must not be <c>null</c> or whitespace.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task whose result is <c>true</c> if the key exists and has not expired;
    /// otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
