using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.IntegrationTests.Api.Middlewares
{
    public class CorrelationIdMiddlewareIntegrationTests
    {
        [Fact]
        public async Task Response_ShouldContainCorrelationIdHeader()
        {
            using TestServer server = new(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ILoggerMessage, TestLoggerMessage>();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<OnlineVoting.Api.Middlewares.CorrelationIdMiddleware>();

                    app.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        await context.Response.WriteAsync("ok");
                    });
                }));

            using HttpClient client = server.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/");

            Assert.True(response.Headers.TryGetValues(
                OnlineVoting.Api.Middlewares.CorrelationIdMiddleware.CorrelationIdHeaderName,
                out IEnumerable<string>? values));

            string correlationId = Assert.Single(values);

            Assert.False(string.IsNullOrWhiteSpace(correlationId));
            Assert.Equal(32, correlationId.Length);
        }

        [Fact]
        public async Task Response_ShouldPreserveValidCorrelationIdHeader()
        {
            using TestServer server = new(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ILoggerMessage, TestLoggerMessage>();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<OnlineVoting.Api.Middlewares.CorrelationIdMiddleware>();

                    app.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        await context.Response.WriteAsync("ok");
                    });
                }));

            using HttpClient client = server.CreateClient();

            const string correlationId = "integration-request-123";

            using HttpRequestMessage request = new(HttpMethod.Get, "/");
            request.Headers.Add(
                OnlineVoting.Api.Middlewares.CorrelationIdMiddleware.CorrelationIdHeaderName,
                correlationId);

            HttpResponseMessage response = await client.SendAsync(request);

            Assert.True(response.Headers.TryGetValues(
                OnlineVoting.Api.Middlewares.CorrelationIdMiddleware.CorrelationIdHeaderName,
                out IEnumerable<string>? values));

            Assert.Equal(correlationId, Assert.Single(values));
        }

        private sealed class TestLoggerMessage : ILoggerMessage
        {
            public void LogDebug(string message)
            {
            }

            public void LogError(string message)
            {
            }

            public void LogInfo(string message)
            {
            }

            public void LogWarn(string message)
            {
            }

            public void LogError(Exception exception, string message)
            {
            }
        }
    }
}