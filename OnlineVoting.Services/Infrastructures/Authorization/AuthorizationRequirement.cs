using Microsoft.AspNetCore.Authorization;

namespace OnlineVoting.Services.Infrastructures.Authorization
{
    public class AuthorizationRequirement : IAuthorizationRequirement
    {
        public int Success { get; set; }
    }
}
