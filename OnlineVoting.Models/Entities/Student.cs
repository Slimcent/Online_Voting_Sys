using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Student : ITracker
    {
        public Guid Id { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Email { get; set; }
        public string? Sex { get; set; }
        public string? RegNo { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public int DepartmentId { get; set; }
        public virtual User? User { get; set; }
        public virtual RegisteredVoter? RegisteredVoter { get; set; }
        public virtual Department? Department { get; set; }
    }
}
