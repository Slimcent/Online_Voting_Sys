using OnlineVoting.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Entities
{
    public class Address : ITracker
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? PlotNo { get; set; }
        public string? StreetName { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Nationality { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual User User { get; set; } = null!;
    }
}