
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class ElectionType : ITracker, IAuditable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public ICollection<Election> Elections { get; set; } = [];
    }
}