using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineVoting.Api.Middlewares;
using OnlineVoting.Models.Context;
using System.Net;

namespace OnlineVoting.Tests.IntegrationTests.Api.ServiceExtension
{
    public class HealthCheckTests
    {
        [Fact]
        public async Task Live_WhenDatabaseIsUnavailable_ShouldReturnOk()
        {
            using IHost host = await CreateHost(databaseAvailable: false);

            HttpClient client = host.GetTestClient();

            using HttpResponseMessage response = await client.GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Ready_WhenDatabaseIsAvailableAndRedisIsDisabled_ShouldReturnOk()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpResponseMessage response = await client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Ready_WhenDatabaseIsUnavailable_ShouldReturnServiceUnavailable()
        {
            using IHost host = await CreateHost(databaseAvailable: false);

            HttpClient client = host.GetTestClient();

            using HttpResponseMessage response = await client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact]
        public async Task Ready_WhenRedisIsEnabledAndUnavailable_ShouldReturnServiceUnavailable()
        {
            using IHost host = await CreateHost(distributedEnabled: true, redisAvailable: false);

            HttpClient client = host.GetTestClient();

            using HttpResponseMessage response = await client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact]
        public async Task Ready_WhenRedisIsEnabledAndAvailable_ShouldReturnOk()
        {
            using IHost host = await CreateHost(distributedEnabled: true);

            HttpClient client = host.GetTestClient();

            using HttpResponseMessage response = await client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static async Task<IHost> CreateHost(bool databaseAvailable = true, bool distributedEnabled = false,
            bool redisAvailable = true)
        {
            Dictionary<string, string?> configurationValues = new()
            {
                ["Caching:DistributedEnabled"] = distributedEnabled.ToString()
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

            IHost host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();

                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddRouting();

                        services.AddScoped<VotingDbContext>(_ =>
                        {
                            DbContextOptionsBuilder<VotingDbContext> optionsBuilder = new();

                            if (databaseAvailable)
                            {
                                optionsBuilder.UseSqlite("Data Source=:memory:");
                            }
                            else
                            {
                                string databasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(),
                                    "health-check.db");

                                optionsBuilder.UseSqlite($"Data Source={databasePath}");
                            }

                            return new VotingDbContext(optionsBuilder.Options, null!, null!);
                        });

                        if (distributedEnabled)
                        {
                            if (redisAvailable)
                            {
                                services.AddDistributedMemoryCache();
                            }
                            else
                            {
                                services.AddSingleton<IDistributedCache, UnavailableDistributedCache>();
                            }
                        }

                        services.ConfigureHealthChecks(configuration);
                    });

                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                            {
                                Predicate = _ => false
                            });

                            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                            {
                                Predicate = healthCheck => healthCheck.Tags.Contains("ready")
                            });
                        });
                    });
                })
                .StartAsync();

            return host;
        }

        private sealed class UnavailableDistributedCache : IDistributedCache
        {
            public byte[]? Get(string key)
            {
                throw new InvalidOperationException("Redis is unavailable.");
            }

            public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
            {
                throw new InvalidOperationException("Redis is unavailable.");
            }

            public void Refresh(string key)
            {
                throw new InvalidOperationException("Redis is unavailable.");
            }

            public Task RefreshAsync(string key, CancellationToken token = default)
            {
                throw new InvalidOperationException("Redis is unavailable.");
            }

            public void Remove(string key)
            {
                throw new InvalidOperationException("Redis is unavailable.");
            }

            public Task RemoveAsync(string key, CancellationToken token = default)
            {
                throw new InvalidOperationException("Redis is unavailable.");
            }

            public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
            {
                throw new InvalidOperationException("Redis is unavailable.");
            }

            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
                CancellationToken token = default)
            {
                throw new InvalidOperationException("Redis is unavailable.");
            }
        }
    }
}