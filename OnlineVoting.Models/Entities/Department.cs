using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Department : ITracker
    {
        public Department()
        {
            Students = new HashSet<Student>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int FacultyId { get; set; }
        public bool Activated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Faculty Faculty { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}
