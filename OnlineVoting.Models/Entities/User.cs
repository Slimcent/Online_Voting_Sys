using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Interfaces;

namespace OnlineVoting.Models.Entities
{
    public class User : IdentityUser, ITracker
    {
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? RecoveryEmail { get; set; }
        public bool Active { get; set; }
        public int UserTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual Staff? Staff { get; set; }
        public virtual UserType UserType { get; set; }
        public virtual ICollection<ApplicationUserClaim> Claims { get; set; }
        public virtual ICollection<IdentityUserLogin<string>> Logins { get; set; }
        public virtual ICollection<IdentityUserToken<string>> Tokens { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}