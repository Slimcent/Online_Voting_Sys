using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class Role : IdentityRole, ITracker
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
