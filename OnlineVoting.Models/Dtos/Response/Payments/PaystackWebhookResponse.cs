using System.Text.Json.Serialization;

namespace OnlineVoting.Models.Dtos.Response.Payments
{
    public class PaystackWebhookResponse
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public PaystackWebhookData? Data { get; set; }
    }

    public class PaystackWebhookData
    {
        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;
    }
}
