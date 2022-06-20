using OnlineVoting.Models.Interfaces;
namespace OnlineVoting.Models.Entities
{
    public class Position : ITracker
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
