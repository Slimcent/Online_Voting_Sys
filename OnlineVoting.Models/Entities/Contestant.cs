using OnlineVoting.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVoting.Models.Entities
{
    public class Contestant : ITracker
    {
        public Guid Id { get; set; }        
        public Guid UserId { get; set; }
        public Guid StudentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual User? User { get; set; }
        public virtual Student? Student { get; set; }
    }
}
