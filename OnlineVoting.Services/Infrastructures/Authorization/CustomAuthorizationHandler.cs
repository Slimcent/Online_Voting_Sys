using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Infrastructures.Extensions;
using SchMgr_FUTO.Data.Interfaces;
using VotingSystem.Data.Interfaces;


namespace OnlineVoting.Services.Infrastructures.Authorization
{
    public class CustomAuthorizationHandler : AuthorizationHandler<AuthorizationRequirement>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IRepository<ApplicationUserClaim> _userClaimRepo;
        private readonly IRepository<ApplicationUserRole> _userRoleRepo;

        public CustomAuthorizationHandler(IHttpContextAccessor contextAccessor, IUnitOfWork unitOfWork)
        {
            _contextAccessor = contextAccessor;
            _userRoleRepo = unitOfWork.GetRepository<ApplicationUserRole>();
            _userClaimRepo = unitOfWork.GetRepository<ApplicationUserClaim>();
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
                return;

            string? userId = context.User.GetUserId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                context.Fail();
                return;
            }

            Endpoint? endpoint = _contextAccessor.HttpContext?.GetEndpoint();

            string? routeClaim = endpoint?
                .Metadata
                .GetMetadata<EndpointNameMetadata>()?
                .EndpointName.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(routeClaim))
            {
                context.Fail();
                return;
            }

            IEnumerable<ApplicationUserRole> userRoles = await _userRoleRepo.GetQueryable()
                .Include(userRole => userRole.Role).ThenInclude(role => role.RoleClaims)
                .Where(userRole => userRole.UserId == userId)
                .ToListAsync();

            IEnumerable<ApplicationUserClaim> userClaims = await _userClaimRepo.GetByAsync(userClaim => userClaim.UserId == userId);

            bool userRoleHasClaim = userRoles.Any(userRole => userRole.Active && userRole.Role.Active
                && userRole.Role.RoleClaims.Any(roleClaim => roleClaim.Active
                && roleClaim.ClaimValue == routeClaim));

            bool userClaimHasClaim = userClaims.Any(userClaim => userClaim.Active && userClaim.ClaimValue == routeClaim);

            if (userRoleHasClaim || userClaimHasClaim)
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
        }
    }
}