using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class ApplicationUserClaim : IdentityUserClaim<string>, ITracker
    {
        public bool Active { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}