using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Faculty : ITracker
    {
        public Faculty()
        {
            Departments = new HashSet<Department>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool Activated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ICollection<Department> Departments { get; set; }
    }
}
