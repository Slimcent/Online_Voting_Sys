using Microsoft.Extensions.Caching.Memory;
using Moq;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using System.Net;
using System.Text;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.UnitTests.Services.Implementation
{
    public class IpGeolocationServiceTests
    {
        [Fact]
        public async Task GetLocation_WithSuccessfulResponse_ShouldReturnLocation()
        {
            string responseContent = """
            {
                "success": true,
                "country": "Germany",
                "region": "North Rhine-Westphalia",
                "city": "Paderborn",
                "latitude": 51.7189,
                "longitude": 8.7575
            }
            """;

            TestHttpMessageHandler messageHandler = new(
                HttpStatusCode.OK,
                responseContent);

            HttpClient httpClient = new(messageHandler)
            {
                BaseAddress = new Uri("https://ipwho.is/")
            };

            MemoryCache memoryCache = new(new MemoryCacheOptions());

            Mock<ILoggerMessage> loggerMessage = new();
            Mock<IServiceFactory> serviceFactory = new();

            serviceFactory
                .Setup(factory => factory.GetService<ILoggerMessage>())
                .Returns(loggerMessage.Object);

            IpGeolocationService service = new(
                httpClient,
                memoryCache,
                serviceFactory.Object);

            IpGeolocationResponse? result = await service.GetLocation("8.8.8.8");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Germany", result.Country);
            Assert.Equal("North Rhine-Westphalia", result.Region);
            Assert.Equal("Paderborn", result.City);
            Assert.Equal(51.7189, result.Latitude);
            Assert.Equal(8.7575, result.Longitude);
        }

        [Fact]
        public async Task GetLocation_WithUnsuccessfulResponse_ShouldReturnNull()
        {
            string responseContent = """
            {
                "success": false,
                "message": "Invalid IP address"
            }
            """;

            TestHttpMessageHandler messageHandler = new(
                HttpStatusCode.OK,
                responseContent);

            HttpClient httpClient = new(messageHandler)
            {
                BaseAddress = new Uri("https://ipwho.is/")
            };

            MemoryCache memoryCache = new(new MemoryCacheOptions());

            Mock<ILoggerMessage> loggerMessage = new();
            Mock<IServiceFactory> serviceFactory = new();

            serviceFactory
                .Setup(factory => factory.GetService<ILoggerMessage>())
                .Returns(loggerMessage.Object);

            IpGeolocationService service = new(
                httpClient,
                memoryCache,
                serviceFactory.Object);

            IpGeolocationResponse? result = await service.GetLocation("invalid");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetLocation_WhenLocationIsCached_ShouldNotMakeSecondRequest()
        {
            string responseContent = """
            {
                "success": true,
                "country": "Germany",
                "region": "North Rhine-Westphalia",
                "city": "Paderborn",
                "latitude": 51.7189,
                "longitude": 8.7575
            }
            """;

            TestHttpMessageHandler messageHandler = new(
                HttpStatusCode.OK,
                responseContent);

            HttpClient httpClient = new(messageHandler)
            {
                BaseAddress = new Uri("https://ipwho.is/")
            };

            MemoryCache memoryCache = new(new MemoryCacheOptions());

            Mock<ILoggerMessage> loggerMessage = new();
            Mock<IServiceFactory> serviceFactory = new();

            serviceFactory
                .Setup(factory => factory.GetService<ILoggerMessage>())
                .Returns(loggerMessage.Object);

            IpGeolocationService service = new(
                httpClient,
                memoryCache,
                serviceFactory.Object);

            IpGeolocationResponse? firstResult = await service.GetLocation("8.8.8.8");
            IpGeolocationResponse? secondResult = await service.GetLocation("8.8.8.8");

            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.Equal(1, messageHandler.RequestCount);
        }

        private sealed class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _responseContent;

            public TestHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
            {
                _statusCode = statusCode;
                _responseContent = responseContent;
            }

            public int RequestCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                RequestCount++;

                HttpResponseMessage response = new(_statusCode)
                {
                    Content = new StringContent(
                        _responseContent,
                        Encoding.UTF8,
                        "application/json")
                };

                return Task.FromResult(response);
            }
        }
    }
}