using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OnlineVoting.Api.Middlewares;
using Microsoft.Extensions.Options;
using System.Net;

namespace OnlineVoting.Tests.IntegrationTests.Api.ServiceExtension
{
    public class SecurityHeadersTests
    {
        [Fact]
        public async Task Response_ShouldContainSecurityHeaders()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpResponseMessage response = await client.GetAsync("/security-test");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("nosniff", GetHeader(response, "X-Content-Type-Options"));
            Assert.Equal("no-referrer", GetHeader(response, "Referrer-Policy"));
            Assert.Equal("DENY", GetHeader(response, "X-Frame-Options"));
            Assert.Equal("frame-ancestors 'none'; object-src 'none'; base-uri 'none'",
                GetHeader(response, "Content-Security-Policy"));
            Assert.Equal("camera=(), microphone=(), geolocation=(), payment=(), usb=()",
                GetHeader(response, "Permissions-Policy"));
        }

        [Fact]
        public async Task Response_WhenHstsIsDisabled_ShouldNotContainHstsHeader()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();
            client.BaseAddress = new Uri("https://localhost");

            using HttpResponseMessage response = await client.GetAsync("/security-test");

            Assert.True(response.IsSuccessStatusCode);
            Assert.False(response.Headers.Contains("Strict-Transport-Security"));
        }

        [Fact]
        public async Task Response_WhenHstsIsEnabled_ShouldContainHstsHeader()
        {
            using IHost host = await CreateHost(hstsEnabled: true);

            HttpClient client = host.GetTestClient();
            client.BaseAddress = new Uri("https://example.com");

            using HttpResponseMessage response = await client.GetAsync("/security-test");

            Assert.True(response.IsSuccessStatusCode);

            string hstsHeader = GetHeader(response, "Strict-Transport-Security");

            Assert.Equal("max-age=2592000", hstsHeader);
        }

        [Fact]
        public async Task Request_WhenHttpsRedirectionIsDisabled_ShouldNotRedirect()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpResponseMessage response = await client.GetAsync("/security-test");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.Location);
        }

        [Fact]
        public async Task Request_WhenHttpsRedirectionIsEnabled_ShouldRedirectToHttps()
        {
            using IHost host = await CreateHost(httpsRedirectionEnabled: true);

            HttpClient client = host.GetTestClient();

            using HttpRequestMessage request = new(HttpMethod.Get, "/security-test");

            using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Equal("https", response.Headers.Location.Scheme);
            Assert.Equal("/security-test", response.Headers.Location.AbsolutePath);
        }

        private static string GetHeader(HttpResponseMessage response, string headerName)
        {
            Assert.True(response.Headers.TryGetValues(headerName, out IEnumerable<string>? values));

            return Assert.Single(values);
        }

        [Fact]
        public async Task Configuration_WhenHstsMaxAgeDaysIsInvalid_ShouldFailAtStartup()
        {
            await Assert.ThrowsAsync<OptionsValidationException>(() =>
                CreateHost(hstsMaxAgeDays: 0));
        }

        private static async Task<IHost> CreateHost(bool hstsEnabled = false, bool httpsRedirectionEnabled = false, int hstsMaxAgeDays = 30)
        {
            Dictionary<string, string?> configurationValues = new()
            {
                ["SecurityHeaders:HttpsRedirectionEnabled"] = httpsRedirectionEnabled.ToString(),
                ["SecurityHeaders:Hsts:Enabled"] = hstsEnabled.ToString(),
                ["SecurityHeaders:Hsts:MaxAgeDays"] = hstsMaxAgeDays.ToString(),
                ["SecurityHeaders:Hsts:IncludeSubDomains"] = "false",
                ["SecurityHeaders:Hsts:Preload"] = "false"
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
                        services.ConfigureSecurityHeaders(configuration);

                        services.AddHttpsRedirection(options =>
                        {
                            options.HttpsPort = 443;
                        });
                    });

                    webBuilder.Configure(app =>
                    {
                        app.UseSecurityHeaders();

                        app.Run(async context =>
                        {
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"result\":\"security-test\"}");
                        });
                    });
                })
                .StartAsync();

            return host;
        }
    }
}
