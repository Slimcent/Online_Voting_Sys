using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Implementation;
using OnlineVoting.Caching.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.UnitTests.Caching
{
    public class HybridCacheServiceTests
    {
        [Fact]
        public async Task GetOrCreate_WhenValueIsCached_ShouldNotCallFactoryAgain()
        {
            using ServiceProvider serviceProvider = CreateServiceProvider();

            ICacheService cacheService = serviceProvider.GetRequiredService<ICacheService>();

            int factoryCallCount = 0;

            ValueTask<string> Factory(CancellationToken cancellationToken)
            {
                factoryCallCount++;
                return ValueTask.FromResult("Computer Science");
            }

            string firstResult = await cacheService.GetOrCreate("test:faculty:1", Factory);
            string secondResult = await cacheService.GetOrCreate("test:faculty:1", Factory);

            Assert.Equal("Computer Science", firstResult);
            Assert.Equal("Computer Science", secondResult);
            Assert.Equal(1, factoryCallCount);
        }

        [Fact]
        public async Task Set_WhenValueIsCached_ShouldReturnCachedValue()
        {
            using ServiceProvider serviceProvider = CreateServiceProvider();

            ICacheService cacheService = serviceProvider.GetRequiredService<ICacheService>();

            await cacheService.Set("test:faculty:2", "Engineering");

            int factoryCallCount = 0;

            string result = await cacheService.GetOrCreate("test:faculty:2", cancellationToken =>
            {
                factoryCallCount++;
                return ValueTask.FromResult("Database Value");
            });

            Assert.Equal("Engineering", result);
            Assert.Equal(0, factoryCallCount);
        }

        [Fact]
        public async Task Remove_WhenKeyIsRemoved_ShouldCallFactoryAgain()
        {
            using ServiceProvider serviceProvider = CreateServiceProvider();

            ICacheService cacheService = serviceProvider.GetRequiredService<ICacheService>();

            int factoryCallCount = 0;

            ValueTask<string> Factory(CancellationToken cancellationToken)
            {
                factoryCallCount++;
                return ValueTask.FromResult($"Value {factoryCallCount}");
            }

            string firstResult = await cacheService.GetOrCreate("test:faculty:3", Factory);

            await cacheService.Remove("test:faculty:3");

            string secondResult = await cacheService.GetOrCreate("test:faculty:3", Factory);

            Assert.Equal("Value 1", firstResult);
            Assert.Equal("Value 2", secondResult);
            Assert.Equal(2, factoryCallCount);
        }

        [Fact]
        public async Task RemoveByTag_WhenTagIsRemoved_ShouldCallFactoryAgain()
        {
            using ServiceProvider serviceProvider = CreateServiceProvider();

            ICacheService cacheService = serviceProvider.GetRequiredService<ICacheService>();

            CacheEntryOptions options = new()
            {
                Tags = ["faculty"]
            };

            int factoryCallCount = 0;

            ValueTask<string> Factory(CancellationToken cancellationToken)
            {
                factoryCallCount++;
                return ValueTask.FromResult($"Value {factoryCallCount}");
            }

            string firstResult = await cacheService.GetOrCreate("test:faculty:4", Factory, options);

            await cacheService.RemoveByTag("faculty");

            string secondResult = await cacheService.GetOrCreate("test:faculty:4", Factory, options);

            Assert.Equal("Value 1", firstResult);
            Assert.Equal("Value 2", secondResult);
            Assert.Equal(2, factoryCallCount);
        }

        [Fact]
        public async Task GetOrCreate_WhenCachingIsDisabled_ShouldAlwaysCallFactory()
        {
            using ServiceProvider serviceProvider = CreateServiceProvider(enabled: false);

            ICacheService cacheService = serviceProvider.GetRequiredService<ICacheService>();

            int factoryCallCount = 0;

            ValueTask<string> Factory(CancellationToken cancellationToken)
            {
                factoryCallCount++;
                return ValueTask.FromResult($"Value {factoryCallCount}");
            }

            string firstResult = await cacheService.GetOrCreate("test:faculty:5", Factory);
            string secondResult = await cacheService.GetOrCreate("test:faculty:5", Factory);

            Assert.Equal("Value 1", firstResult);
            Assert.Equal("Value 2", secondResult);
            Assert.Equal(2, factoryCallCount);
        }

        [Fact]
        public async Task GetOrCreate_WhenDistributedModeIsUsedWithoutDistributedCache_ShouldThrow()
        {
            using ServiceProvider serviceProvider = CreateServiceProvider();

            ICacheService cacheService = serviceProvider.GetRequiredService<ICacheService>();

            CacheEntryOptions options = new()
            {
                Mode = CacheMode.Distributed
            };

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await cacheService.GetOrCreate("test:faculty:6",
                    cancellationToken => ValueTask.FromResult("Computer Science"), options));

            Assert.Equal("Distributed cache mode requires distributed caching to be enabled.", exception.Message);
        }

        [Fact]
        public async Task Set_WhenCacheFails_ShouldLogErrorAndNotThrow()
        {
            Mock<HybridCache> cache = new();
            Mock<ILoggerMessage> loggerMessage = new();

            InvalidOperationException exception = new("Cache unavailable.");

            cache.Setup(cache => cache.SetAsync("test:faculty:7", "Engineering",
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromException(exception));

            HybridCacheService cacheService = new(
                cache.Object,
                Options.Create(new CacheOptions
                {
                    Enabled = true,
                    DefaultExpiration = TimeSpan.FromMinutes(10),
                    DefaultLocalCacheExpiration = TimeSpan.FromMinutes(2),
                    DistributedEnabled = false,
                    RedisConnectionStringName = "Redis"
                }),
                loggerMessage.Object);

            await cacheService.Set("test:faculty:7", "Engineering");

            loggerMessage.Verify(logger => logger.LogError(exception, "Failed to set cache entry for key test:faculty:7."), Times.Once);
        }

        [Fact]
        public async Task Remove_WhenCacheFails_ShouldLogErrorAndNotThrow()
        {
            Mock<HybridCache> cache = new();
            Mock<ILoggerMessage> loggerMessage = new();

            InvalidOperationException exception = new("Cache unavailable.");

            cache.Setup(cache => cache.RemoveAsync("test:faculty:8",
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromException(exception));

            HybridCacheService cacheService = new(cache.Object,
                Options.Create(new CacheOptions
                {
                    Enabled = true,
                    DefaultExpiration = TimeSpan.FromMinutes(10),
                    DefaultLocalCacheExpiration = TimeSpan.FromMinutes(2),
                    DistributedEnabled = false,
                    RedisConnectionStringName = "Redis"
                }),
                loggerMessage.Object);

            await cacheService.Remove("test:faculty:8");

            loggerMessage.Verify(logger => logger.LogError(exception, "Failed to remove cache entry for key test:faculty:8."), Times.Once);
        }

        [Fact]
        public async Task RemoveByTag_WhenCacheFails_ShouldLogErrorAndNotThrow()
        {
            Mock<HybridCache> cache = new();
            Mock<ILoggerMessage> loggerMessage = new();

            InvalidOperationException exception = new("Cache unavailable.");

            cache.Setup(cache => cache.RemoveByTagAsync("faculty",
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromException(exception));

            HybridCacheService cacheService = new(
                cache.Object,
                Options.Create(new CacheOptions
                {
                    Enabled = true,
                    DefaultExpiration = TimeSpan.FromMinutes(10),
                    DefaultLocalCacheExpiration = TimeSpan.FromMinutes(2),
                    DistributedEnabled = false,
                    RedisConnectionStringName = "Redis"
                }),
                loggerMessage.Object);

            await cacheService.RemoveByTag("faculty");

            loggerMessage.Verify(logger => logger.LogError(exception, "Failed to remove cache entries for tag faculty."), Times.Once);
        }

        [Fact]
        public async Task Set_WhenOperationIsCancelled_ShouldRethrowCancellation()
        {
            Mock<HybridCache> cache = new();
            Mock<ILoggerMessage> loggerMessage = new();

            using CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();

            cache.Setup(cache => cache.SetAsync("test:faculty:9", "Engineering",
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<IEnumerable<string>>(),
                cancellationTokenSource.Token))
            .Returns(ValueTask.FromCanceled(cancellationTokenSource.Token));

            HybridCacheService cacheService = new(
                cache.Object,
                Options.Create(new CacheOptions
                {
                    Enabled = true,
                    DefaultExpiration = TimeSpan.FromMinutes(10),
                    DefaultLocalCacheExpiration = TimeSpan.FromMinutes(2),
                    DistributedEnabled = false,
                    RedisConnectionStringName = "Redis"
                }),
                loggerMessage.Object);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await cacheService.Set("test:faculty:9", "Engineering", cancellationToken: cancellationTokenSource.Token));

            loggerMessage.Verify(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Remove_WhenOperationIsCancelled_ShouldRethrowCancellation()
        {
            Mock<HybridCache> cache = new();
            Mock<ILoggerMessage> loggerMessage = new();

            using CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();

            cache.Setup(cache => cache.RemoveAsync("test:faculty:10", cancellationTokenSource.Token))
                .Returns(ValueTask.FromCanceled(cancellationTokenSource.Token));

            HybridCacheService cacheService = new(
                cache.Object,
                Options.Create(new CacheOptions
                {
                    Enabled = true,
                    DefaultExpiration = TimeSpan.FromMinutes(10),
                    DefaultLocalCacheExpiration = TimeSpan.FromMinutes(2),
                    DistributedEnabled = false,
                    RedisConnectionStringName = "Redis"
                }),
                loggerMessage.Object);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await cacheService.Remove("test:faculty:10", cancellationTokenSource.Token));

            loggerMessage.Verify(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveByTag_WhenOperationIsCancelled_ShouldRethrowCancellation()
        {
            Mock<HybridCache> cache = new();
            Mock<ILoggerMessage> loggerMessage = new();

            using CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.Cancel();

            cache.Setup(cache => cache.RemoveByTagAsync("faculty", cancellationTokenSource.Token))
                .Returns(ValueTask.FromCanceled(cancellationTokenSource.Token));

            HybridCacheService cacheService = new(
                cache.Object,
                Options.Create(new CacheOptions
                {
                    Enabled = true,
                    DefaultExpiration = TimeSpan.FromMinutes(10),
                    DefaultLocalCacheExpiration = TimeSpan.FromMinutes(2),
                    DistributedEnabled = false,
                    RedisConnectionStringName = "Redis"
                }),
                loggerMessage.Object);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await cacheService.RemoveByTag("faculty", cancellationTokenSource.Token));

            loggerMessage.Verify(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        }

        private static ServiceProvider CreateServiceProvider(bool enabled = true)
        {
            ServiceCollection services = new();

            services.AddHybridCache();

            services.AddSingleton<IOptions<CacheOptions>>(Options.Create(new CacheOptions
            {
                Enabled = enabled,
                DefaultExpiration = TimeSpan.FromMinutes(10),
                DefaultLocalCacheExpiration = TimeSpan.FromMinutes(2),
                DistributedEnabled = false,
                RedisConnectionStringName = "Redis"
            }));

            Mock<ILoggerMessage> loggerMessage = new();

            services.AddSingleton(loggerMessage.Object);
            services.AddSingleton<ICacheService, HybridCacheService>();

            return services.BuildServiceProvider();
        }
    }
}