using Microsoft.AspNetCore.Authorization;

namespace OnlineVoting.Services.Infrastructures
{
    public class AuthorizationRequirment : IAuthorizationRequirement
    {
        public int Success { get; set; }
    }
}
