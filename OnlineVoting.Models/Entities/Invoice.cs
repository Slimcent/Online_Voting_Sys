using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Invoice : ITracker, IAuditable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PositionApplicationId { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string PayerFirstName { get; set; } = string.Empty;
        public string PayerLastName { get; set; } = string.Empty;
        public string? PayerEmail { get; set; }
        public string? RegistrationNumber { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int InvoiceStatusId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual PositionApplication PositionApplication { get; set; } = null!;
        public virtual InvoiceStatus InvoiceStatus { get; set; } = null!;
        public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = [];
    }
}