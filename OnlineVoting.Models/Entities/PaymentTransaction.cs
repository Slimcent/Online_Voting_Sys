using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class PaymentTransaction : ITracker, IAuditable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InvoiceId { get; set; } = string.Empty;
        public int PaymentGatewayId { get; set; }
        public int PaymentStatusId { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public string? ProviderReference { get; set; }
        public string? CheckoutUrl { get; set; }
        public Guid StudentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string PayerFirstName { get; set; } = string.Empty;
        public string PayerLastName { get; set; } = string.Empty;
        public string? PayerEmail { get; set; }
        public string? RegistrationNumber { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Invoice Invoice { get; set; } = null!;
        public virtual PaymentGateway PaymentGateway { get; set; } = null!;
        public virtual PaymentStatus PaymentStatus { get; set; } = null!;
    }
}