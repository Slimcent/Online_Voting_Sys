using Microsoft.Extensions.Options;
using OnlineVoting.Models.Configurations;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request.Payments;
using OnlineVoting.Models.Dtos.Response.Payments;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces.Payments;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Implementation.Payments
{
    public class PaystackPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackSettings _settings;
        private readonly ILoggerMessage _loggerMessage;

        public string Code => ApplicationConstants.PaymentGateways.Paystack;

        public PaystackPaymentGateway(HttpClient httpClient, IOptions<PaystackSettings> options, ILoggerMessage loggerMessage)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _loggerMessage = loggerMessage;
        }

        public async Task<Result<GatewayPaymentInitializationResponse>> InitiatePayment(GatewayPaymentInitializationRequest request)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                _loggerMessage.LogError("Paystack payment initialization failed because the secret key was not configured.");
                return Result<GatewayPaymentInitializationResponse>.ValidationError("Paystack payment gateway is not configured.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _loggerMessage.LogWarn($"Paystack payment initialization failed for payment reference {request.PaymentReference} because payer email was not available.");
                return Result<GatewayPaymentInitializationResponse>.ValidationError("Payer email is required for Paystack payment initialization.");
            }

            PaystackInitializeRequest paystackRequest = new PaystackInitializeRequest
            {
                Email = request.Email,
                Amount = ConvertToSubunit(request.Amount),
                Reference = request.PaymentReference,
                Currency = request.Currency,
                CallbackUrl = _settings.CallbackUrl
            };

            using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "transaction/initialize");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
            httpRequest.Content = JsonContent.Create(paystackRequest);

            HttpResponseMessage httpResponse;

            try
            {
                httpResponse = await _httpClient.SendAsync(httpRequest);
            }
            catch (HttpRequestException exception)
            {
                _loggerMessage.LogError($"Paystack payment initialization encountered a network error for payment reference {request.PaymentReference}. Error: {exception.Message}");
                throw;
            }
            catch (TaskCanceledException exception)
            {
                _loggerMessage.LogError($"Paystack payment initialization timed out for payment reference {request.PaymentReference}. Error: {exception.Message}");
                throw;
            }

            string responseContent = await httpResponse.Content.ReadAsStringAsync();

            PaystackInitializeResponse? paystackResponse;

            try
            {
                paystackResponse = JsonSerializer.Deserialize<PaystackInitializeResponse>(responseContent);
            }
            catch (JsonException exception)
            {
                _loggerMessage.LogError($"Paystack returned an invalid initialization response for payment reference {request.PaymentReference}. Error: {exception.Message}");
                return Result<GatewayPaymentInitializationResponse>.ValidationError("Paystack returned an invalid payment initialization response.");
            }

            if (!httpResponse.IsSuccessStatusCode || paystackResponse is null || !paystackResponse.Status || paystackResponse.Data is null)
            {
                string errorMessage = paystackResponse?.Message ?? "Paystack payment initialization failed.";

                _loggerMessage.LogWarn($"Paystack payment initialization failed for payment reference {request.PaymentReference}. Response: {errorMessage}");

                return Result<GatewayPaymentInitializationResponse>.ValidationError(errorMessage);
            }

            if (string.IsNullOrWhiteSpace(paystackResponse.Data.AuthorizationUrl))
            {
                _loggerMessage.LogWarn($"Paystack payment initialization returned no authorization URL for payment reference {request.PaymentReference}.");
                return Result<GatewayPaymentInitializationResponse>.ValidationError("Paystack did not return a checkout URL.");
            }

            GatewayPaymentInitializationResponse response = new GatewayPaymentInitializationResponse
            {
                ProviderReference = paystackResponse.Data.Reference,
                CheckoutUrl = paystackResponse.Data.AuthorizationUrl
            };

            _loggerMessage.LogInfo($"Paystack payment initialized successfully for payment reference {request.PaymentReference}.");

            return Result<GatewayPaymentInitializationResponse>.Success(response);
        }

        public async Task<Result<GatewayPaymentVerificationResponse>> VerifyPayment(string paymentReference)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                _loggerMessage.LogError("Paystack payment verification failed because the secret key was not configured.");
                return Result<GatewayPaymentVerificationResponse>.ValidationError("Paystack payment gateway is not configured.");
            }

            if (string.IsNullOrWhiteSpace(paymentReference))
            {
                _loggerMessage.LogWarn("Paystack payment verification failed because the payment reference was not provided.");
                return Result<GatewayPaymentVerificationResponse>.ValidationError("Payment reference is required.");
            }

            using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"transaction/verify/{Uri.EscapeDataString(paymentReference)}");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SecretKey);

            HttpResponseMessage httpResponse;

            try
            {
                httpResponse = await _httpClient.SendAsync(httpRequest);
            }
            catch (HttpRequestException exception)
            {
                _loggerMessage.LogError($"Paystack payment verification encountered a network error for payment reference {paymentReference}. Error: {exception.Message}");
                throw;
            }
            catch (TaskCanceledException exception)
            {
                _loggerMessage.LogError($"Paystack payment verification timed out for payment reference {paymentReference}. Error: {exception.Message}");
                throw;
            }

            string responseContent = await httpResponse.Content.ReadAsStringAsync();

            PaystackVerificationResponse? paystackResponse;

            try
            {
                paystackResponse = JsonSerializer.Deserialize<PaystackVerificationResponse>(responseContent);
            }
            catch (JsonException exception)
            {
                _loggerMessage.LogError($"Paystack returned an invalid verification response for payment reference {paymentReference}. Error: {exception.Message}");
                return Result<GatewayPaymentVerificationResponse>.ValidationError("Paystack returned an invalid payment verification response.");
            }

            if (!httpResponse.IsSuccessStatusCode || paystackResponse is null || !paystackResponse.Status || paystackResponse.Data is null)
            {
                string errorMessage = paystackResponse?.Message ?? "Paystack payment verification failed.";

                _loggerMessage.LogWarn($"Paystack payment verification failed for payment reference {paymentReference}. Response: {errorMessage}");

                return Result<GatewayPaymentVerificationResponse>.ValidationError(errorMessage);
            }

            GatewayPaymentVerificationResponse response = new GatewayPaymentVerificationResponse
            {
                PaymentReference = paystackResponse.Data.Reference,
                ProviderReference = paystackResponse.Data.Id.ToString(),
                Status = GetPaymentStatus(paystackResponse.Data.Status),
                Amount = ConvertFromSubunit(paystackResponse.Data.Amount),
                Currency = paystackResponse.Data.Currency,
                PaidAt = paystackResponse.Data.PaidAt
            };

            _loggerMessage.LogInfo($"Paystack payment verification completed for payment reference {paymentReference} with status {paystackResponse.Data.Status}.");

            return Result<GatewayPaymentVerificationResponse>.Success(response);
        }

        public Result<GatewayPaymentWebhookResponse> ProcessWebhook(string payload, string signature)
        {
            if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            {
                _loggerMessage.LogError("Paystack webhook processing failed because the secret key was not configured.");
                return Result<GatewayPaymentWebhookResponse>.ValidationError("Paystack payment gateway is not configured.");
            }

            if (string.IsNullOrWhiteSpace(payload))
            {
                _loggerMessage.LogWarn("Paystack webhook processing failed because the webhook payload was empty.");
                return Result<GatewayPaymentWebhookResponse>.ValidationError("Webhook payload is required.");
            }

            if (string.IsNullOrWhiteSpace(signature))
            {
                _loggerMessage.LogWarn("Paystack webhook processing failed because the webhook signature was not provided.");
                return Result<GatewayPaymentWebhookResponse>.Unauthorized("Invalid webhook signature.");
            }

            if (!IsValidWebhookSignature(payload, signature))
            {
                _loggerMessage.LogWarn("Paystack webhook processing failed because the webhook signature was invalid.");
                return Result<GatewayPaymentWebhookResponse>.Unauthorized("Invalid webhook signature.");
            }

            PaystackWebhookResponse? webhookResponse;

            try
            {
                webhookResponse = JsonSerializer.Deserialize<PaystackWebhookResponse>(payload);
            }
            catch (JsonException exception)
            {
                _loggerMessage.LogError($"Paystack webhook processing failed because the webhook payload was invalid. Error: {exception.Message}");
                return Result<GatewayPaymentWebhookResponse>.ValidationError("Invalid Paystack webhook payload.");
            }

            if (webhookResponse is null)
            {
                _loggerMessage.LogWarn("Paystack webhook processing failed because the webhook payload could not be read.");
                return Result<GatewayPaymentWebhookResponse>.ValidationError("Invalid Paystack webhook payload.");
            }

            if (string.IsNullOrWhiteSpace(webhookResponse.Event))
            {
                _loggerMessage.LogWarn("Paystack webhook processing failed because the webhook event was not provided.");
                return Result<GatewayPaymentWebhookResponse>.ValidationError("Webhook event is required.");
            }

            if (webhookResponse.Data is null || string.IsNullOrWhiteSpace(webhookResponse.Data.Reference))
            {
                _loggerMessage.LogWarn($"Paystack webhook event {webhookResponse.Event} was received without a payment reference.");
                return Result<GatewayPaymentWebhookResponse>.ValidationError("Payment reference was not found in the webhook payload.");
            }

            GatewayPaymentWebhookResponse response = new GatewayPaymentWebhookResponse
            {
                Event = webhookResponse.Event,
                PaymentReference = webhookResponse.Data.Reference
            };

            return Result<GatewayPaymentWebhookResponse>.Success(response);
        }

        private bool IsValidWebhookSignature(string payload, string signature)
        {
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(_settings.SecretKey);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            using HMACSHA512 hmac = new HMACSHA512(secretKeyBytes);

            byte[] hashBytes = hmac.ComputeHash(payloadBytes);
            string expectedSignature = Convert.ToHexString(hashBytes).ToLowerInvariant();

            byte[] expectedSignatureBytes = Encoding.UTF8.GetBytes(expectedSignature);
            byte[] actualSignatureBytes = Encoding.UTF8.GetBytes(signature.Trim().ToLowerInvariant());

            if (expectedSignatureBytes.Length != actualSignatureBytes.Length)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(expectedSignatureBytes, actualSignatureBytes);
        }

        private static string GetPaymentStatus(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "success" => ApplicationConstants.GatewayPaymentStatuses.Succeeded,
                "failed" => ApplicationConstants.GatewayPaymentStatuses.Failed,
                "abandoned" => ApplicationConstants.GatewayPaymentStatuses.Cancelled,
                _ => ApplicationConstants.GatewayPaymentStatuses.Pending
            };
        }

        private static long ConvertToSubunit(decimal amount)
        {
            return Convert.ToInt64(decimal.Round(amount * 100, 0, MidpointRounding.AwayFromZero));
        }

        private static decimal ConvertFromSubunit(long amount)
        {
            return amount / 100m;
        }
    }
}