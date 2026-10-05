using System.Text.Json.Serialization;

namespace OnlineVoting.Models.Dtos.Response.Payments
{
    public class PaystackInitializeResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public PaystackInitializeData? Data { get; set; }
    }

    public class GatewayPaymentVerificationResponse
    {
        public string PaymentReference { get; set; } = string.Empty;
        public string? ProviderReference { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }

    public class PaystackInitializeData
    {
        [JsonPropertyName("authorization_url")]
        public string? AuthorizationUrl { get; set; }

        [JsonPropertyName("access_code")]
        public string? AccessCode { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }

    public class PaystackVerificationResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public PaystackVerificationData? Data { get; set; }
    }

    public class PaystackVerificationData
    {
        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("paid_at")]
        public DateTime? PaidAt { get; set; }
    }

}
