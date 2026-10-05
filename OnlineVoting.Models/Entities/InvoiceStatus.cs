using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class InvoiceStatus : ITracker, IAuditable
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; } = [];
    }
}