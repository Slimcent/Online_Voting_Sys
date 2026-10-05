namespace OnlineVoting.Models.Dtos.Response.Payments
{
    public class GatewayPaymentWebhookResponse
    {
        public string Event { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
    }
}
