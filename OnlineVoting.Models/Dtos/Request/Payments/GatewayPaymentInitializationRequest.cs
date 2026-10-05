namespace OnlineVoting.Models.Dtos.Request.Payments
{
    public class GatewayPaymentInitializationRequest
    {
        public string PaymentReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PayerFirstName { get; set; } = string.Empty;
        public string PayerLastName { get; set; } = string.Empty;
    }
}
