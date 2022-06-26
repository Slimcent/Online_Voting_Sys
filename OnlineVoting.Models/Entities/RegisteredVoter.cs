using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class RegisteredVoter : ITracker
    {
        public RegisteredVoter()
        {
            IsDeActivated = false;
        }

        public Guid Id { get; set; }
        public Guid? StudentId { get; set; }
        public string? VotingCode { get; set; }
        public int DepartmentId { get; set; }
        public bool IsDeActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Student? Student { get; set; }
        public virtual Department? Department { get; set; }
    }
}
