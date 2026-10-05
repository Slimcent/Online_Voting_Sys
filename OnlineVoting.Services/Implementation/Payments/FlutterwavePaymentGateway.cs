using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Results;
using OnlineVoting.Models.Dtos.Request.Payments;
using OnlineVoting.Models.Dtos.Response.Payments;
using OnlineVoting.Services.Interfaces.Payments;

namespace OnlineVoting.Services.Implementation.Payments
{
    public class FlutterwavePaymentGateway : IPaymentGateway
    {
        public string Code => ApplicationConstants.PaymentGateways.Flutterwave;

        public Task<Result<GatewayPaymentInitializationResponse>> InitiatePayment(GatewayPaymentInitializationRequest request)
        {
            return Task.FromResult(Result<GatewayPaymentInitializationResponse>.ValidationError("Flutterwave payment initialization has not been implemented yet."));
        }

        public Task<Result<GatewayPaymentVerificationResponse>> VerifyPayment(string paymentReference)
        {
            return Task.FromResult(Result<GatewayPaymentVerificationResponse>.ValidationError("Flutterwave payment verification has not been implemented yet."));
        }

        public Result<GatewayPaymentWebhookResponse> ProcessWebhook(string payload, string signature)
        {
            return Result<GatewayPaymentWebhookResponse>.ValidationError("Flutterwave webhook processing has not been implemented yet.");
        }
    }
}
