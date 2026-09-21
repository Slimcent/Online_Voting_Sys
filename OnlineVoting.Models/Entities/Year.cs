using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Year : ITracker, IAuditable
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public ICollection<Election> Elections { get; set; } = [];
    }
}
