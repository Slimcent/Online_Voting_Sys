using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Identity;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Infrastructures;
using System.Security.Claims;


namespace OnlineVoting.Services.Helpers
{
    public class CustomAuthorizationHandler : AuthorizationHandler<AuthorizationRequirment>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        UserManager<User> _userManager { get; }
        RoleManager<Role> _roleManager { get; }

        public CustomAuthorizationHandler(IHttpContextAccessor contextAccessor, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _contextAccessor = contextAccessor;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected async override Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRequirment requirement)
        {
            if (context.User.Identity.Name == null)
            {
                return Task.CompletedTask;
            }

            User user = await _userManager.FindByNameAsync(context.User.Identity.Name);

            Endpoint? endpoint = _contextAccessor.HttpContext.GetEndpoint();
            string endpointName = endpoint?.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName;
            string routeClaim = endpointName;
            List<Claim> userClaims = (await _userManager.GetClaimsAsync(user)).ToList();
            List<string> userRoles = (await _userManager.GetRolesAsync(user)).ToList();

            foreach (string userRole in userRoles)
            {
                Role role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    IList<Claim> roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (Claim roleClaim in roleClaims)
                    {
                        if (roleClaim.Value == routeClaim)
                        {
                            context.Succeed(requirement);
                            return Task.CompletedTask;
                        }
                    }
                }
            }
            foreach (var userClaim in userClaims)
            {
                if (userClaim.Value == routeClaim)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }
    }
}
