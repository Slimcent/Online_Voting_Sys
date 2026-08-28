using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Gender : IAuditable
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}