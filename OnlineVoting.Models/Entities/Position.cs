using OnlineVoting.Models.Interfaces;
namespace OnlineVoting.Models.Entities
{
    public class Position : ITracker, IAuditable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        public long? FacultyId { get; set; }

        public long? DepartmentId { get; set; }

        public bool Active { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        public virtual Faculty? Faculty { get; set; }

        public virtual Department? Department { get; set; }

        public ICollection<ElectionPosition> ElectionPositions { get; set; } = [];
    }
}
