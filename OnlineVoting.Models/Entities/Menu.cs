using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Menu : IAuditable
    {
        public long Id { get; set; }
        public string? Name { get; set; }
    }
}
