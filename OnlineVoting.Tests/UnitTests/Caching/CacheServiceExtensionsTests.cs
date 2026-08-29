using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Caching.Extensions;
using OnlineVoting.Caching.Implementation;
using OnlineVoting.Caching.Interfaces;

namespace OnlineVoting.Tests.UnitTests.Caching
{
    public class CacheServiceExtensionsTests
    {
        [Fact]
        public void AddApplicationCaching_ShouldRegisterCacheService()
        {
            IConfiguration configuration = CreateConfiguration();

            ServiceCollection services = new();

            services.AddApplicationCaching(configuration);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            ICacheService cacheService = serviceProvider.GetRequiredService<ICacheService>();

            Assert.IsType<HybridCacheService>(cacheService);
        }

        [Fact]
        public void AddApplicationCaching_ShouldBindCacheOptions()
        {
            IConfiguration configuration = CreateConfiguration();

            ServiceCollection services = new();

            services.AddApplicationCaching(configuration);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            CacheOptions options = serviceProvider.GetRequiredService<IOptions<CacheOptions>>().Value;

            Assert.True(options.Enabled);
            Assert.Equal(TimeSpan.FromMinutes(10), options.DefaultExpiration);
            Assert.Equal(TimeSpan.FromMinutes(2), options.DefaultLocalCacheExpiration);
            Assert.False(options.DistributedEnabled);
            Assert.Equal("Redis", options.RedisConnectionStringName);
        }

        [Fact]
        public void AddApplicationCaching_WhenDefaultExpirationIsInvalid_ShouldThrow()
        {
            Dictionary<string, string?> values = new()
            {
                ["Caching:Enabled"] = "true",
                ["Caching:DefaultExpiration"] = "00:00:00",
                ["Caching:DefaultLocalCacheExpiration"] = "00:02:00",
                ["Caching:DistributedEnabled"] = "false",
                ["Caching:RedisConnectionStringName"] = "Redis"
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();

            ServiceCollection services = new();

            services.AddApplicationCaching(configuration);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
                serviceProvider.GetRequiredService<IOptions<CacheOptions>>().Value);

            Assert.Contains("Caching:DefaultExpiration must be greater than zero.", exception.Failures);
        }

        [Fact]
        public void AddApplicationCaching_WhenLocalExpirationIsGreaterThanExpiration_ShouldThrow()
        {
            Dictionary<string, string?> values = new()
            {
                ["Caching:Enabled"] = "true",
                ["Caching:DefaultExpiration"] = "00:01:00",
                ["Caching:DefaultLocalCacheExpiration"] = "00:02:00",
                ["Caching:DistributedEnabled"] = "false",
                ["Caching:RedisConnectionStringName"] = "Redis"
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();

            ServiceCollection services = new();

            services.AddApplicationCaching(configuration);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() =>
                serviceProvider.GetRequiredService<IOptions<CacheOptions>>().Value);

            Assert.Contains("Caching:DefaultLocalCacheExpiration cannot be greater than DefaultExpiration.",
                exception.Failures);
        }

        [Fact]
        public void AddApplicationCaching_WhenDistributedCacheIsEnabledWithoutConnectionString_ShouldThrow()
        {
            Dictionary<string, string?> values = new()
            {
                ["Caching:Enabled"] = "true",
                ["Caching:DefaultExpiration"] = "00:10:00",
                ["Caching:DefaultLocalCacheExpiration"] = "00:02:00",
                ["Caching:DistributedEnabled"] = "true",
                ["Caching:RedisConnectionStringName"] = "Redis"
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();

            ServiceCollection services = new();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                services.AddApplicationCaching(configuration));

            Assert.Equal("Redis is enabled but connection string 'Redis' was not found.", exception.Message);
        }

        private static IConfiguration CreateConfiguration()
        {
            Dictionary<string, string?> values = new()
            {
                ["Caching:Enabled"] = "true",
                ["Caching:DefaultExpiration"] = "00:10:00",
                ["Caching:DefaultLocalCacheExpiration"] = "00:02:00",
                ["Caching:DistributedEnabled"] = "false",
                ["Caching:RedisConnectionStringName"] = "Redis"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}