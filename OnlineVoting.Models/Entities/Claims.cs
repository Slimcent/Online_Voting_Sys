using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Claims : IAuditable
    {
        public long Id { get; set; }
        public string? Claim { get; set; }
        public long? MenuId { get; set; }
        public virtual Menu? Menu { get; set; }
    }
}