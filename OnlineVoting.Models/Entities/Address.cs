using OnlineVoting.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Entities
{
    public class Address : ITracker
    {
        [Key]
        public Guid Id { get; set; }
        public int? PlotNo { get; set; }
        public string? StreetName { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Nationality { get; set; }
        public Guid? StaffId { get; set; }
        public Guid? StudentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Student? Student { get; set; }
        public virtual Staff? Staff { get; set; }
    }
}
