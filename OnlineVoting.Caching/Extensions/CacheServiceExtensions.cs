using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Implementation;
using OnlineVoting.Caching.Interfaces;
using StackExchange.Redis;
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
            .Validate(options => options.RedisConnectTimeout > TimeSpan.Zero,
                "Caching:RedisConnectTimeout must be greater than zero.")
            .Validate(options => options.RedisOperationTimeout > TimeSpan.Zero,
                "Caching:RedisOperationTimeout must be greater than zero.")
            .Validate(options => options.RedisConnectRetry >= 0,
                "Caching:RedisConnectRetry cannot be negative.")
            .ValidateOnStart();

            CacheOptions cacheOptions = cacheSection.Get<CacheOptions>() ?? new CacheOptions();

            if (cacheOptions.DistributedEnabled)
            {
                string? redisConnectionString = configuration.GetConnectionString(cacheOptions.RedisConnectionStringName);

                if (string.IsNullOrWhiteSpace(redisConnectionString))
                    throw new InvalidOperationException($"Redis is enabled but connection string '{cacheOptions.RedisConnectionStringName}' was not found.");

                ConfigurationOptions redisConfiguration = ConfigurationOptions.Parse(redisConnectionString);

                redisConfiguration.AbortOnConnectFail = false;
                redisConfiguration.ConnectTimeout = (int)cacheOptions.RedisConnectTimeout.TotalMilliseconds;
                redisConfiguration.SyncTimeout = (int)cacheOptions.RedisOperationTimeout.TotalMilliseconds;
                redisConfiguration.AsyncTimeout = (int)cacheOptions.RedisOperationTimeout.TotalMilliseconds;
                redisConfiguration.ConnectRetry = cacheOptions.RedisConnectRetry;
                redisConfiguration.BacklogPolicy = BacklogPolicy.FailFast;

                services.AddStackExchangeRedisCache(options =>
                {
                    options.ConfigurationOptions = redisConfiguration;
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