using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineVoting.Api.Middlewares;
using System.IO.Compression;
using System.Net.Http.Headers;

namespace OnlineVoting.Tests.IntegrationTests.Api.ServiceExtension
{
    public class ResponseCompressionTests
    {
        [Fact]
        public async Task Response_WhenBrotliIsAccepted_ShouldBeBrotliCompressed()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpRequestMessage request = new(HttpMethod.Get, "/compression-test");
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("br", response.Content.Headers.ContentEncoding);

            byte[] compressedContent = await response.Content.ReadAsByteArrayAsync();

            using MemoryStream compressedStream = new(compressedContent);
            using BrotliStream brotliStream = new(compressedStream, CompressionMode.Decompress);
            using StreamReader reader = new(brotliStream);

            string content = await reader.ReadToEndAsync();

            Assert.Contains("response-compression-test", content);
        }

        [Fact]
        public async Task Response_WhenGzipIsAccepted_ShouldBeGzipCompressed()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpRequestMessage request = new(HttpMethod.Get, "/compression-test");
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("gzip", response.Content.Headers.ContentEncoding);

            byte[] compressedContent = await response.Content.ReadAsByteArrayAsync();

            using MemoryStream compressedStream = new(compressedContent);
            using GZipStream gzipStream = new(compressedStream, CompressionMode.Decompress);
            using StreamReader reader = new(gzipStream);

            string content = await reader.ReadToEndAsync();

            Assert.Contains("response-compression-test", content);
        }

        [Fact]
        public async Task Response_WhenCompressionIsNotRequested_ShouldNotBeCompressed()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();

            using HttpResponseMessage response = await client.GetAsync("/compression-test");

            Assert.True(response.IsSuccessStatusCode);
            Assert.Empty(response.Content.Headers.ContentEncoding);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Contains("response-compression-test", content);
        }

        [Fact]
        public async Task Response_WhenRequestUsesHttps_ShouldNotBeCompressed()
        {
            using IHost host = await CreateHost();

            HttpClient client = host.GetTestClient();
            client.BaseAddress = new Uri("https://localhost");

            using HttpRequestMessage request = new(HttpMethod.Get, "/compression-test");
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Empty(response.Content.Headers.ContentEncoding);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Contains("response-compression-test", content);
        }

        [Fact]
        public async Task Response_WhenHttpsCompressionIsEnabled_ShouldBeCompressed()
        {
            using IHost host = await CreateHost(enableForHttps: true);

            HttpClient client = host.GetTestClient();
            client.BaseAddress = new Uri("https://localhost");

            using HttpRequestMessage request = new(HttpMethod.Get, "/compression-test");
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("br", response.Content.Headers.ContentEncoding);

            byte[] compressedContent = await response.Content.ReadAsByteArrayAsync();

            using MemoryStream compressedStream = new(compressedContent);
            using BrotliStream brotliStream = new(compressedStream, CompressionMode.Decompress);
            using StreamReader reader = new(brotliStream);

            string content = await reader.ReadToEndAsync();

            Assert.Contains("response-compression-test", content);
        }

        private static async Task<IHost> CreateHost(bool enableForHttps = false)
        {
            Dictionary<string, string?> configurationValues = new()
            {
                ["ResponseCompression:EnableForHttps"] = enableForHttps.ToString()
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
                        services.ConfigureResponseCompression(configuration);
                    });

                    webBuilder.Configure(app =>
                    {
                        app.UseResponseCompression();

                        app.Run(async context =>
                        {
                            context.Response.ContentType = "application/json";

                            string response = string.Join(",", Enumerable.Repeat("\"response-compression-test\":\"This response contains enough repeated content to test compression.\"",
                                100));

                            await context.Response.WriteAsync($"{{{response}}}");
                        });
                    });
                })
                .StartAsync();

            return host;
        }
    }
}
