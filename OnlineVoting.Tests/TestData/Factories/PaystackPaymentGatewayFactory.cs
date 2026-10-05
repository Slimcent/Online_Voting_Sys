using Microsoft.Extensions.Options;
using Moq;
using OnlineVoting.Models.Configurations;
using OnlineVoting.Services.Implementation.Payments;
using System.Net;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public class PaystackPaymentGatewayFactory
    {
        public Mock<ILoggerMessage> LoggerMessage { get; }

        public PaystackSettings Settings { get; }

        public PaystackPaymentGatewayFactory()
        {
            LoggerMessage = new Mock<ILoggerMessage>();

            Settings = new PaystackSettings
            {
                BaseUrl = "https://api.paystack.co/",
                SecretKey = "test-secret-key",
                CallbackUrl = "https://frontend.test/payment/verify"
            };
        }

        public PaystackPaymentGateway CreateGateway(Func<HttpRequestMessage, HttpResponseMessage>? handler = null)
        {
            HttpClient httpClient;

            if (handler is null)
            {
                httpClient = new HttpClient();
            }
            else
            {
                TestHttpMessageHandler messageHandler = new TestHttpMessageHandler(handler);

                httpClient = new HttpClient(messageHandler);
            }

            httpClient.BaseAddress = new Uri(Settings.BaseUrl);

            return new PaystackPaymentGateway(
                httpClient,
                Options.Create(Settings),
                LoggerMessage.Object);
        }

        private sealed class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

            public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_handler(request));
            }
        }
    }
}