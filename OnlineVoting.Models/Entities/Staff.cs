using OnlineVoting.Models.Enums;
using OnlineVoting.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace OnlineVoting.Models.Entities
{
    public class Staff : ITracker
    {
        public Staff()
        {
            IsDeleted = false;
        }

        [Key]
        public Guid Id { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? PhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string? UserId { get; set; }
        public virtual User? User { get; set; }
        public virtual Address? Address { get; set; }
    }
}
