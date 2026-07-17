using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class UserType : ITracker
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
