using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Caching.Implementation
{
    public class HybridCacheService : ICacheService
    {
        private readonly HybridCache _cache;
        private readonly CacheOptions _cacheOptions;
        private readonly ILoggerMessage _loggerMessage;

        public HybridCacheService(HybridCache cache, IOptions<CacheOptions> cacheOptions, ILoggerMessage loggerMessage)
        {
            _cache = cache;
            _cacheOptions = cacheOptions.Value;
            _loggerMessage = loggerMessage;
        }

        public ValueTask<T> GetOrCreate<T>(string key, Func<CancellationToken, ValueTask<T>> factory, CacheEntryOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (!_cacheOptions.Enabled)
                return factory(cancellationToken);

            HybridCacheEntryOptions hybridCacheEntryOptions = CreateOptions(options);

            return _cache.GetOrCreateAsync(key, factory, hybridCacheEntryOptions, options?.Tags, cancellationToken);
        }

        public async ValueTask Set<T>(string key, T value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (!_cacheOptions.Enabled)
                return;

            HybridCacheEntryOptions hybridCacheEntryOptions = CreateOptions(options);

            try
            {
                await _cache.SetAsync(key, value, hybridCacheEntryOptions, options?.Tags, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError(exception, $"Failed to set cache entry for key {key}.");
            }
        }

        public async ValueTask Remove(string key, CancellationToken cancellationToken = default)
        {
            if (!_cacheOptions.Enabled)
                return;

            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError(exception, $"Failed to remove cache entry for key {key}.");
            }
        }

        public async ValueTask RemoveByTag(string tag, CancellationToken cancellationToken = default)
        {
            if (!_cacheOptions.Enabled)
                return;

            try
            {
                await _cache.RemoveByTagAsync(tag, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _loggerMessage.LogError(exception, $"Failed to remove cache entries for tag {tag}.");
            }
        }

        private HybridCacheEntryOptions CreateOptions(CacheEntryOptions? options)
        {
            CacheEntryOptions cacheEntryOptions = options ?? new CacheEntryOptions();

            if (cacheEntryOptions.Mode == CacheMode.Distributed && !_cacheOptions.DistributedEnabled)
                throw new InvalidOperationException("Distributed cache mode requires distributed caching to be enabled.");

            return new HybridCacheEntryOptions
            {
                Expiration = cacheEntryOptions.Expiration ?? _cacheOptions.DefaultExpiration,
                LocalCacheExpiration = cacheEntryOptions.LocalCacheExpiration ?? _cacheOptions.DefaultLocalCacheExpiration,
                Flags = GetFlags(cacheEntryOptions.Mode)
            };
        }

        private static HybridCacheEntryFlags GetFlags(CacheMode mode)
        {
            return mode switch
            {
                CacheMode.Hybrid => HybridCacheEntryFlags.None,
                CacheMode.Local => HybridCacheEntryFlags.DisableDistributedCache,
                CacheMode.Distributed => HybridCacheEntryFlags.DisableLocalCache,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
    }
}