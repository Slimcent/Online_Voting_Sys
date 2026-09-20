using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Department : ITracker, IAuditable
    {
        public Department()
        {
            Students = new HashSet<Student>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public long FacultyId { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Faculty Faculty { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}