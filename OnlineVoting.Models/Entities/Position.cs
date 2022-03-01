using OnlineVoting.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.Entities
{
    public class Position : ITracker
    {
        public Position()
        {
            GetContestants = new HashSet<Contestant>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Contestant>? GetContestants { get; set; }
    }
}
