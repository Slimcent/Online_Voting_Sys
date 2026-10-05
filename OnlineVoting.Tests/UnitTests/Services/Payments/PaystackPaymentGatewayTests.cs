using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request.Payments;
using OnlineVoting.Models.Dtos.Response.Payments;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Implementation.Payments;
using OnlineVoting.Tests.TestData.Factories;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OnlineVoting.Tests.UnitTests.Services.Payments
{
    public class PaystackPaymentGatewayTests
    {
        [Fact]
        public async Task InitiatePayment_WithSuccessfulPaystackResponse_ShouldReturnCheckoutInformation()
        {
            string responseJson = """
            {
                "status": true,
                "message": "Authorization URL created",
                "data": {
                    "authorization_url": "https://checkout.paystack.com/test",
                    "access_code": "ACCESS_123",
                    "reference": "PAY-123"
                }
            }
            """;

            PaystackPaymentGatewayFactory factory = new PaystackPaymentGatewayFactory();

            PaystackPaymentGateway gateway = factory.CreateGateway(request =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal("/transaction/initialize", request.RequestUri?.AbsolutePath);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                };
            });

            GatewayPaymentInitializationRequest request = new GatewayPaymentInitializationRequest
            {
                PaymentReference = "PAY-123",
                Amount = 5000m,
                Currency = "NGN",
                Email = "student@example.com",
                PayerFirstName = "Test",
                PayerLastName = "Student"
            };

            Result<GatewayPaymentInitializationResponse> result = await gateway.InitiatePayment(request);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("PAY-123", result.Value.ProviderReference);
            Assert.Equal("https://checkout.paystack.com/test", result.Value.CheckoutUrl);
        }

        [Fact]
        public async Task InitiatePayment_ShouldSendAmountInSubunit()
        {
            string responseJson = """
            {
                "status": true,
                "message": "Authorization URL created",
                "data": {
                    "authorization_url": "https://checkout.paystack.com/test",
                    "access_code": "ACCESS_123",
                    "reference": "PAY-123"
                }
            }
            """;

            PaystackPaymentGatewayFactory factory = new PaystackPaymentGatewayFactory();

            PaystackPaymentGateway gateway = factory.CreateGateway(request =>
            {
                string body = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();

                using JsonDocument document = JsonDocument.Parse(body);

                Assert.Equal(500000L, document.RootElement.GetProperty("amount").GetInt64());
                Assert.Equal("PAY-123", document.RootElement.GetProperty("reference").GetString());
                Assert.Equal("NGN", document.RootElement.GetProperty("currency").GetString());
                Assert.Equal("student@example.com", document.RootElement.GetProperty("email").GetString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                };
            });

            GatewayPaymentInitializationRequest request = new GatewayPaymentInitializationRequest
            {
                PaymentReference = "PAY-123",
                Amount = 5000m,
                Currency = "NGN",
                Email = "student@example.com"
            };

            Result<GatewayPaymentInitializationResponse> result = await gateway.InitiatePayment(request);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task VerifyPayment_WithSuccessfulPayment_ShouldReturnNormalizedPayment()
        {
                    string responseJson = """
            {
                "status": true,
                "message": "Verification successful",
                "data": {
                    "id": 123456,
                    "status": "success",
                    "reference": "PAY-123",
                    "amount": 500000,
                    "currency": "NGN",
                    "paid_at": "2026-10-03T20:00:00.000Z"
                }
            }
            """;

            PaystackPaymentGatewayFactory factory = new PaystackPaymentGatewayFactory();

            PaystackPaymentGateway gateway = factory.CreateGateway(request =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal("/transaction/verify/PAY-123", request.RequestUri?.AbsolutePath);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                };
            });

            Result<GatewayPaymentVerificationResponse> result = await gateway.VerifyPayment("PAY-123");

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("PAY-123", result.Value.PaymentReference);
            Assert.Equal(ApplicationConstants.GatewayPaymentStatuses.Succeeded, result.Value.Status);
            Assert.Equal(5000m, result.Value.Amount);
            Assert.Equal("NGN", result.Value.Currency);
        }

        [Fact]
        public void ProcessWebhook_WithInvalidSignature_ShouldReturnUnauthorized()
        {
            PaystackPaymentGatewayFactory factory = new PaystackPaymentGatewayFactory();

            PaystackPaymentGateway gateway = factory.CreateGateway();

            string payload = """
            {
                "event": "charge.success",
                "data": {
                    "reference": "PAY-123"
                }
            }
            """;

            Result<GatewayPaymentWebhookResponse> result = gateway.ProcessWebhook(payload, "invalid-signature");

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void ProcessWebhook_WithValidSignature_ShouldReturnPaymentReference()
        {
            PaystackPaymentGatewayFactory factory = new PaystackPaymentGatewayFactory();

            string payload = """
            {
                "event": "charge.success",
                "data": {
                    "reference": "PAY-123"
                }
            }
            """;

            using HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(factory.Settings.SecretKey));

            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            string signature = Convert.ToHexString(hash).ToLowerInvariant();

            PaystackPaymentGateway gateway = factory.CreateGateway();

            Result<GatewayPaymentWebhookResponse> result = gateway.ProcessWebhook(payload, signature);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("charge.success", result.Value.Event);
            Assert.Equal("PAY-123", result.Value.PaymentReference);
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
