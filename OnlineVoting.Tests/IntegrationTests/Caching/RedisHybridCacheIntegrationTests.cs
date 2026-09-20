using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Extensions;
using OnlineVoting.Caching.Interfaces;
using Testcontainers.Redis;

namespace OnlineVoting.Tests.IntegrationTests.Caching
{
    public class RedisHybridCacheIntegrationTests : IAsyncLifetime
    {
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7.4-alpine").Build();

        public async Task InitializeAsync()
        {
            await _redisContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _redisContainer.DisposeAsync();
        }

        [Fact]
        public async Task GetOrCreate_WithNewCacheInstance_ShouldReadValueFromRedis()
        {
            string cacheKey = "integration:faculty:1";
            int firstFactoryCallCount = 0;

            using ServiceProvider firstServiceProvider = CreateServiceProvider();

            ICacheService firstCacheService = firstServiceProvider.GetRequiredService<ICacheService>();

            string firstResult = await firstCacheService.GetOrCreate(cacheKey, cancellationToken =>
            {
                firstFactoryCallCount++;

                return ValueTask.FromResult("Computer Science");
            });

            Assert.Equal("Computer Science", firstResult);
            Assert.Equal(1, firstFactoryCallCount);

            IDistributedCache distributedCache = firstServiceProvider.GetRequiredService<IDistributedCache>();

            byte[]? distributedValue = null;

            for (int attempt = 0; attempt < 20 && distributedValue is null; attempt++)
            {
                distributedValue = await distributedCache.GetAsync(cacheKey);

                if (distributedValue is null)
                    await Task.Delay(100);
            }

            Assert.NotNull(distributedValue);

            using ServiceProvider secondServiceProvider = CreateServiceProvider();

            ICacheService secondCacheService = secondServiceProvider.GetRequiredService<ICacheService>();

            int secondFactoryCallCount = 0;

            string secondResult = await secondCacheService.GetOrCreate(cacheKey, cancellationToken =>
            {
                secondFactoryCallCount++;

                return ValueTask.FromResult("Database Value");
            });

            Assert.Equal("Computer Science", secondResult);
            Assert.Equal(0, secondFactoryCallCount);
        }

        [Fact]
        public async Task RemoveByTag_WithNewCacheInstance_ShouldTreatRedisValueAsStale()
        {
            string cacheKey = "integration:faculty:tagged:1";
            string cacheTag = "integration:faculty";

            CacheEntryOptions cacheEntryOptions = new()
            {
                Tags = [cacheTag]
            };

            using ServiceProvider firstServiceProvider = CreateServiceProvider();

            ICacheService firstCacheService = firstServiceProvider.GetRequiredService<ICacheService>();

            int firstFactoryCallCount = 0;

            string firstResult = await firstCacheService.GetOrCreate(cacheKey, cancellationToken =>
            {
                firstFactoryCallCount++;

                return ValueTask.FromResult("Computer Science");
            }, cacheEntryOptions);

            Assert.Equal("Computer Science", firstResult);
            Assert.Equal(1, firstFactoryCallCount);

            IDistributedCache distributedCache = firstServiceProvider.GetRequiredService<IDistributedCache>();

            byte[]? distributedValue = null;

            for (int attempt = 0; attempt < 20 && distributedValue is null; attempt++)
            {
                distributedValue = await distributedCache.GetAsync(cacheKey);

                if (distributedValue is null)
                    await Task.Delay(100);
            }

            Assert.NotNull(distributedValue);

            await Task.Delay(50);

            await firstCacheService.RemoveByTag(cacheTag);

            using ServiceProvider secondServiceProvider = CreateServiceProvider();

            ICacheService secondCacheService = secondServiceProvider.GetRequiredService<ICacheService>();

            int secondFactoryCallCount = 0;

            string secondResult = await secondCacheService.GetOrCreate(cacheKey, cancellationToken =>
            {
                secondFactoryCallCount++;

                return ValueTask.FromResult("Updated Computer Science");
            }, cacheEntryOptions);

            Assert.Equal("Updated Computer Science", secondResult);
            Assert.Equal(1, secondFactoryCallCount);
        }

        private ServiceProvider CreateServiceProvider()
        {
            Dictionary<string, string?> configurationValues = new()
            {
                ["Caching:Enabled"] = "true",
                ["Caching:DefaultExpiration"] = "00:10:00",
                ["Caching:DefaultLocalCacheExpiration"] = "00:02:00",
                ["Caching:DistributedEnabled"] = "true",
                ["Caching:RedisConnectionStringName"] = "Redis",
                ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString()
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

            ServiceCollection services = new();

            services.AddApplicationCaching(configuration);

            return services.BuildServiceProvider();
        }
    }
}