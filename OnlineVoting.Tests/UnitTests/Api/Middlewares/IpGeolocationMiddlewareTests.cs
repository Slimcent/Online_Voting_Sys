using Microsoft.AspNetCore.Http;
using Moq;
using OnlineVoting.Api.Middlewares;
using OnlineVoting.Models.Configurations;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Services.Interfaces;
using System.Net;

namespace OnlineVoting.Tests.UnitTests.Api.Middlewares
{
    public class IpGeolocationMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WithPublicIpAndPostRequest_ShouldAddLocationToContext()
        {
            DefaultHttpContext context = new();

            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/api/v1/faculties";
            context.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");

            Mock<IIpGeolocationService> ipGeolocationService = new();

            ipGeolocationService.Setup(service => service.GetLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IpGeolocationResponse
                {
                    Success = true,
                    Country = "Germany",
                    Region = "North Rhine-Westphalia",
                    City = "Paderborn",
                    Latitude = 51.7189,
                    Longitude = 8.7575
                });

            bool nextCalled = false;

            RequestDelegate next = _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            };

            IpGeolocationMiddleware middleware = new(next);

            await middleware.InvokeAsync(context, ipGeolocationService.Object);

            Assert.True(nextCalled);
            Assert.Equal("Germany", context.Items[RequestContextKeys.IpCountry]);
            Assert.Equal("North Rhine-Westphalia", context.Items[RequestContextKeys.IpRegion]);
            Assert.Equal("Paderborn", context.Items[RequestContextKeys.IpCity]);
            Assert.Equal(51.7189, context.Items[RequestContextKeys.IpLatitude]);
            Assert.Equal(8.7575, context.Items[RequestContextKeys.IpLongitude]);

            ipGeolocationService.Verify(service => service.GetLocation("8.8.8.8", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithLoopbackIp_ShouldNotCallGeolocationService()
        {
            DefaultHttpContext context = new();

            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/api/v1/faculties";
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Mock<IIpGeolocationService> ipGeolocationService = new();

            bool nextCalled = false;

            RequestDelegate next = _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            };

            IpGeolocationMiddleware middleware = new(next);

            await middleware.InvokeAsync(context, ipGeolocationService.Object);

            Assert.True(nextCalled);

            ipGeolocationService.Verify(service => service.GetLocation(It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithPrivateIp_ShouldNotCallGeolocationService()
        {
            DefaultHttpContext context = new();

            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/api/v1/faculties";
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.10");

            Mock<IIpGeolocationService> ipGeolocationService = new();

            bool nextCalled = false;

            RequestDelegate next = _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            };

            IpGeolocationMiddleware middleware = new(next);

            await middleware.InvokeAsync(context, ipGeolocationService.Object);

            Assert.True(nextCalled);

            ipGeolocationService.Verify(service => service.GetLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithGetRequest_ShouldNotCallGeolocationService()
        {
            DefaultHttpContext context = new();

            context.Request.Method = HttpMethods.Get;
            context.Request.Path = "/api/v1/faculties";
            context.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");

            Mock<IIpGeolocationService> ipGeolocationService = new();

            bool nextCalled = false;

            RequestDelegate next = _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            };

            IpGeolocationMiddleware middleware = new(next);

            await middleware.InvokeAsync(context, ipGeolocationService.Object);

            Assert.True(nextCalled);

            ipGeolocationService.Verify(service => service.GetLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WhenGeolocationFails_ShouldContinueRequest()
        {
            DefaultHttpContext context = new();

            context.Request.Method = HttpMethods.Post;
            context.Request.Path = "/api/v1/faculties";
            context.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");

            Mock<IIpGeolocationService> ipGeolocationService = new();

            ipGeolocationService
                .Setup(service => service.GetLocation("8.8.8.8", It.IsAny<CancellationToken>()))
                .ReturnsAsync((IpGeolocationResponse?)null);

            bool nextCalled = false;

            RequestDelegate next = _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            };

            IpGeolocationMiddleware middleware = new(next);

            await middleware.InvokeAsync(context, ipGeolocationService.Object);

            Assert.True(nextCalled);
            Assert.False(context.Items.ContainsKey(RequestContextKeys.IpCountry));
            Assert.False(context.Items.ContainsKey(RequestContextKeys.IpRegion));
            Assert.False(context.Items.ContainsKey(RequestContextKeys.IpCity));
            Assert.False(context.Items.ContainsKey(RequestContextKeys.IpLatitude));
            Assert.False(context.Items.ContainsKey(RequestContextKeys.IpLongitude));
        }
    }
}