using OnlineVoting.Caching.Configuration;

namespace OnlineVoting.Caching.Interfaces
{
    public interface ICacheService
    {
        ValueTask<T> GetOrCreate<T>(string key, Func<CancellationToken, ValueTask<T>> factory, CacheEntryOptions? options = null,
            CancellationToken cancellationToken = default);

        ValueTask Set<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

        ValueTask Remove(string key, CancellationToken cancellationToken = default);

        ValueTask RemoveByTag(string tag, CancellationToken cancellationToken = default);
    }
}