using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class ApplicationUserRole : IdentityUserRole<string>, ITracker
    {
        public bool Active { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}