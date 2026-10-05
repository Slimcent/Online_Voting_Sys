namespace OnlineVoting.Models.Dtos.Response.Payments
{
    public class GatewayPaymentInitializationResponse
    {
        public string? ProviderReference { get; set; }
        public string? CheckoutUrl { get; set; }
    }
}
