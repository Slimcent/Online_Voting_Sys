using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Implementation;
using OnlineVoting.Caching.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VotingSystem.Logger;

namespace OnlineVoting.Caching.Extensions
{
    public static class CacheServiceExtensions
    {
        public static IServiceCollection AddApplicationCaching(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection cacheSection = configuration.GetSection(CacheOptions.SectionName);

            services.TryAddSingleton<ILoggerMessage, LoggerMessage>();

            services.AddOptions<CacheOptions>().Bind(cacheSection)
                .Validate(options => options.DefaultExpiration > TimeSpan.Zero,
                    "Caching:DefaultExpiration must be greater than zero.")
                .Validate(options => options.DefaultLocalCacheExpiration > TimeSpan.Zero,
                    "Caching:DefaultLocalCacheExpiration must be greater than zero.")
                .Validate(options => options.DefaultLocalCacheExpiration <= options.DefaultExpiration,
                    "Caching:DefaultLocalCacheExpiration cannot be greater than DefaultExpiration.")
                .ValidateOnStart();

            CacheOptions cacheOptions = cacheSection.Get<CacheOptions>() ?? new CacheOptions();

            if (cacheOptions.DistributedEnabled)
            {
                string? redisConnectionString = configuration.GetConnectionString(cacheOptions.RedisConnectionStringName);

                if (string.IsNullOrWhiteSpace(redisConnectionString))
                    throw new InvalidOperationException($"Redis is enabled but connection string '{cacheOptions.RedisConnectionStringName}' was not found.");

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                });
            }

            services.AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = cacheOptions.DefaultExpiration,
                    LocalCacheExpiration = cacheOptions.DefaultLocalCacheExpiration
                };
            });

            services.AddSingleton<ICacheService, HybridCacheService>();

            return services;
        }
    }
}