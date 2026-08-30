using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OnlineVoting.Api.Middlewares;
using System.Net;

namespace OnlineVoting.Tests.IntegrationTests.Api.ServiceExtension
{
    public class CorsTests
    {
        private const string AllowedOrigin = "https://frontend.example.com";
        private const string BlockedOrigin = "https://malicious.example.com";

        [Fact]
        public async Task Request_FromAllowedOrigin_ShouldContainAllowOriginHeader()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpRequestMessage request = new(HttpMethod.Get, "/cors-test");
            request.Headers.Add("Origin", AllowedOrigin);

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(AllowedOrigin, GetHeader(response, "Access-Control-Allow-Origin"));
        }

        [Fact]
        public async Task Request_FromBlockedOrigin_ShouldNotContainAllowOriginHeader()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpRequestMessage request = new(HttpMethod.Get, "/cors-test");
            request.Headers.Add("Origin", BlockedOrigin);

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }

        [Fact]
        public async Task Preflight_FromAllowedOrigin_ShouldReturnCorsHeaders()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpRequestMessage request = new(HttpMethod.Options, "/cors-test");
            request.Headers.Add("Origin", AllowedOrigin);
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "Authorization,Content-Type");

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(AllowedOrigin, GetHeader(response, "Access-Control-Allow-Origin"));

            string allowedMethods = GetHeader(response, "Access-Control-Allow-Methods");
            string allowedHeaders = GetHeader(response, "Access-Control-Allow-Headers");

            Assert.Contains("POST", allowedMethods);
            Assert.Contains("Authorization", allowedHeaders, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Content-Type", allowedHeaders, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Preflight_FromBlockedOrigin_ShouldNotContainCorsHeaders()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpRequestMessage request = new(HttpMethod.Options, "/cors-test");
            request.Headers.Add("Origin", BlockedOrigin);
            request.Headers.Add("Access-Control-Request-Method", "POST");

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }

        [Fact]
        public async Task Request_WhenCorsIsDisabled_ShouldNotContainCorsHeaders()
        {
            using IHost host = await CreateHost(corsEnabled: false);

            HttpClient client = host.GetTestClient();

            using HttpRequestMessage request = new(HttpMethod.Get, "/cors-test");
            request.Headers.Add("Origin", AllowedOrigin);

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }

        [Fact]
        public async Task Configuration_WhenCorsIsEnabledWithoutAllowedOrigins_ShouldFailAtStartup()
        {
            await Assert.ThrowsAsync<OptionsValidationException>(() =>
                CreateHost(corsEnabled: true, includeAllowedOrigin: false));
        }

        [Fact]
        public async Task Configuration_WhenAllowedOriginHasTrailingSlash_ShouldFailAtStartup()
        {
            await Assert.ThrowsAsync<OptionsValidationException>(() =>
                CreateHost(allowedOrigin: "https://frontend.example.com/"));
        }

        private static string GetHeader(HttpResponseMessage response, string headerName)
        {
            Assert.True(response.Headers.TryGetValues(headerName, out IEnumerable<string>? values));

            return Assert.Single(values);
        }
                
        private static async Task<IHost> CreateHost(bool corsEnabled = true, bool includeAllowedOrigin = true, string allowedOrigin = AllowedOrigin)
        {
            Dictionary<string, string?> configurationValues = new()
            {
                ["Cors:Enabled"] = corsEnabled.ToString(),
                ["Cors:AllowedMethods:0"] = "GET",
                ["Cors:AllowedMethods:1"] = "POST",
                ["Cors:AllowedMethods:2"] = "PUT",
                ["Cors:AllowedMethods:3"] = "PATCH",
                ["Cors:AllowedMethods:4"] = "DELETE",
                ["Cors:AllowedHeaders:0"] = "Accept",
                ["Cors:AllowedHeaders:1"] = "Authorization",
                ["Cors:AllowedHeaders:2"] = "Content-Type",
                ["Cors:AllowedHeaders:3"] = "X-Correlation-ID",
                ["Cors:PreflightMaxAgeMinutes"] = "10"
            };

            if (includeAllowedOrigin)
            {
                configurationValues["Cors:AllowedOrigins:0"] = allowedOrigin;
            }

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
                        services.ConfigureCors(configuration);
                    });

                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();

                        app.UseCors("CorsPolicy");

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapMethods("/cors-test",
                                ["GET", "POST", "PUT", "PATCH", "DELETE"],
                                async context =>
                                {
                                    context.Response.ContentType = "application/json";
                                    await context.Response.WriteAsync("{\"result\":\"cors-test\"}");
                                });
                        });
                    });
                })
                .StartAsync();

            return host;
        }
    }
}
