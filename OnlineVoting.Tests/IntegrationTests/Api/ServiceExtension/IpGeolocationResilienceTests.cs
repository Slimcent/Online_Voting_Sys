using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Moq;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using Polly;
using System.Net;
using System.Text;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.IntegrationTests.Api.ServiceExtension
{
    public class IpGeolocationResilienceTests
    {
        [Fact]
        public async Task GetLocation_WhenServerReturnsTransientFailures_ShouldRetryAndSucceed()
        {
            SequenceHttpMessageHandler messageHandler = new(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, HttpStatusCode.OK);

            IIpGeolocationService service = CreateService(messageHandler);

            IpGeolocationResponse? result = await service.GetLocation("8.8.8.8");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, messageHandler.RequestCount);
        }

        [Fact]
        public async Task GetLocation_WhenServerReturnsBadRequest_ShouldNotRetry()
        {
            SequenceHttpMessageHandler messageHandler = new(HttpStatusCode.BadRequest);

            IIpGeolocationService service = CreateService(messageHandler);

            IpGeolocationResponse? result = await service.GetLocation("invalid");

            Assert.Null(result);
            Assert.Equal(1, messageHandler.RequestCount);
        }

        [Fact]
        public async Task GetLocation_WhenRequestTimesOut_ShouldReturnNull()
        {
            DelayedHttpMessageHandler messageHandler = new();

            IIpGeolocationService service = CreateService(messageHandler);

            IpGeolocationResponse? result = await service.GetLocation("8.8.8.8");

            Assert.Null(result);
            Assert.True(messageHandler.RequestCount >= 1);
        }

        private static IIpGeolocationService CreateService(HttpMessageHandler messageHandler)
        {
            ServiceCollection services = new();

            Mock<ILoggerMessage> loggerMessage = new();
            Mock<IServiceFactory> serviceFactory = new();

            serviceFactory.Setup(factory => factory.GetService<ILoggerMessage>())
                .Returns(loggerMessage.Object);

            services.AddMemoryCache();
            services.AddSingleton(serviceFactory.Object);

            services.AddHttpClient<IIpGeolocationService, IpGeolocationService>(client =>
            {
                client.BaseAddress = new Uri("https://ipwho.is/");
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .ConfigurePrimaryHttpMessageHandler(() => messageHandler)
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(1);

                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromMilliseconds(10);
                options.Retry.BackoffType = DelayBackoffType.Constant;
                options.Retry.UseJitter = false;
                options.Retry.DisableForUnsafeHttpMethods();

                options.AttemptTimeout.Timeout = TimeSpan.FromMilliseconds(100);
            });

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<IIpGeolocationService>();
        }

        private sealed class SequenceHttpMessageHandler : HttpMessageHandler
        {
            private readonly Queue<HttpStatusCode> _statusCodes;

            public SequenceHttpMessageHandler(params HttpStatusCode[] statusCodes)
            {
                _statusCodes = new Queue<HttpStatusCode>(statusCodes);
            }

            public int RequestCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestCount++;

                HttpStatusCode statusCode = _statusCodes.Count > 1
                    ? _statusCodes.Dequeue()
                    : _statusCodes.Peek();

                string responseContent = statusCode == HttpStatusCode.OK
                    ? """
                      {
                          "success": true,
                          "country": "Germany",
                          "region": "North Rhine-Westphalia",
                          "city": "Paderborn",
                          "latitude": 51.7189,
                          "longitude": 8.7575
                      }
                      """
                    : """
                      {
                          "success": false,
                          "message": "Request failed"
                      }
                      """;

                HttpResponseMessage response = new(statusCode)
                {
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };

                return Task.FromResult(response);
            }
        }

        private sealed class DelayedHttpMessageHandler : HttpMessageHandler
        {
            public int RequestCount { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestCount++;

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }
    }
}