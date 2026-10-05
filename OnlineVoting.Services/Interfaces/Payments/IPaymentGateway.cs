using OnlineVoting.Models.Dtos.Request.Payments;
using OnlineVoting.Models.Dtos.Response.Payments;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces.Payments
{
    public interface IPaymentGateway
    {
        string Code { get; }
        Task<Result<GatewayPaymentInitializationResponse>> InitiatePayment(GatewayPaymentInitializationRequest request);
        Task<Result<GatewayPaymentVerificationResponse>> VerifyPayment(string paymentReference);
        Result<GatewayPaymentWebhookResponse> ProcessWebhook(string payload, string signature);
    }
}