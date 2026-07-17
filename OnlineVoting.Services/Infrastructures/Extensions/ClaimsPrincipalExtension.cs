using System.Security.Claims;

namespace OnlineVoting.Services.Infrastructures.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst("Id")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static IEnumerable<string> GetRoles(this ClaimsPrincipal user)
        {
            return user.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public static IEnumerable<Claim> GetClaims(this ClaimsPrincipal user)
        {
            return user.Claims;
        }

        public static IEnumerable<string> GetClaimValues(this ClaimsPrincipal user, string claimType)
        {
            return user.FindAll(claimType)
                .Select(claim => claim.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public static bool HasClaimValue(this ClaimsPrincipal user, string claimValue)
        {
            return user.Claims.Any(claim => string.Equals(claim.Value, claimValue, StringComparison.OrdinalIgnoreCase));
        }

        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return user.GetRoles().Any(userRole => string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase));
        }
    }
}