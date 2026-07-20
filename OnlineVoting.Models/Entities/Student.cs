using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Student : ITracker
    {
        public Guid Id { get; set; }
        public string? RegNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public long DepartmentId { get; set; }
        public int GenderId { get; set; }
        public bool Active { get; set; }
        public virtual User? User { get; set; }
        public virtual Gender Gender { get; set; }
        public virtual RegisteredVoter? RegisteredVoter { get; set; }
        public virtual Department? Department { get; set; }
        public virtual Address? Address { get; set; }
    }
}
