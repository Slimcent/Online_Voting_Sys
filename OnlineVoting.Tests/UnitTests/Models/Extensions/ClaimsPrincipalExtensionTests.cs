using OnlineVoting.Models.Extensions;
using System.Security.Claims;

namespace OnlineVoting.Tests.UnitTests.Models.Extensions
{
    public class ClaimsPrincipalExtensionTests
    {
        [Fact]
        public Task GetUsername_WithNameClaim_ShouldReturnUsername()
        {
            ClaimsPrincipal user = CreateClaimsPrincipal(new Claim(ClaimTypes.Name, "adminuser"));

            string? username = user.GetUsername();

            Assert.Equal("adminuser", username);

            return Task.CompletedTask;
        }

        [Fact]
        public Task GetUsername_WithoutNameClaim_ShouldReturnNull()
        {
            ClaimsPrincipal user = CreateClaimsPrincipal();

            string? username = user.GetUsername();

            Assert.Null(username);

            return Task.CompletedTask;
        }

        [Fact]
        public Task GetUserId_WithIdClaim_ShouldReturnIdClaim()
        {
            ClaimsPrincipal user = CreateClaimsPrincipal(
                new Claim("Id", "user-123"),
                new Claim(ClaimTypes.NameIdentifier, "fallback-456"));

            string? userId = user.GetUserId();

            Assert.Equal("user-123", userId);

            return Task.CompletedTask;
        }

        [Fact]
        public Task GetUserId_WithoutIdClaim_ShouldReturnNameIdentifier()
        {
            ClaimsPrincipal user = CreateClaimsPrincipal(new Claim(ClaimTypes.NameIdentifier, "user-456"));

            string? userId = user.GetUserId();

            Assert.Equal("user-456", userId);

            return Task.CompletedTask;
        }

        [Fact]
        public Task GetUserId_WithoutUserIdClaims_ShouldReturnNull()
        {
            ClaimsPrincipal user = CreateClaimsPrincipal();

            string? userId = user.GetUserId();

            Assert.Null(userId);

            return Task.CompletedTask;
        }

        [Fact]
        public Task GetRoles_ShouldReturnDistinctNonEmptyRoles()
        {
            ClaimsPrincipal user = CreateClaimsPrincipal(
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "Student"),
                new Claim(ClaimTypes.Role, string.Empty));

            IEnumerable<string> roles = user.GetRoles();

            Assert.Equal(2, roles.Count());
            Assert.Contains("Admin", roles);
            Assert.Contains("Student", roles);

            return Task.CompletedTask;
        }

        [Fact]
        public Task GetClaims_ShouldReturnAllClaims()
        {
            ClaimsPrincipal user = CreateClaimsPrincipal(
                new Claim(ClaimTypes.Name, "adminuser"),
                new Claim(ClaimTypes.Role, "Admin"));

            IEnumerable<Claim> claims = user.GetClaims();

            Assert.Equal(2, claims.Count());
            Assert.Contains(claims, claim => claim.Type == ClaimTypes.Name && claim.Value == "adminuser");
            Assert.Contains(claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");

            return Task.CompletedTask;
        }


        [Fact]
        public Task GetClaimValues_ShouldReturnDistinctNonEmptyValues()
        {
            const string claimType = "Permission";

            ClaimsPrincipal user = CreateClaimsPrincipal(
                new Claim(claimType, "ManageElection"),
                new Claim(claimType, "manageelection"),
                new Claim(claimType, "ViewElection"),
                new Claim(claimType, string.Empty));

            IEnumerable<string> values = user.GetClaimValues(claimType);

            Assert.Equal(2, values.Count());
            Assert.Contains("ManageElection", values);
            Assert.Contains("ViewElection", values);

            return Task.CompletedTask;
        }

        [Theory]
        [InlineData("ManageElection")]
        [InlineData("manageelection")]
        [InlineData("MANAGEELECTION")]
        public Task HasClaimValue_WithMatchingValue_ShouldReturnTrue(string claimValue)
        {
            ClaimsPrincipal user = CreateClaimsPrincipal(new Claim("Permission", "ManageElection"));

            bool result = user.HasClaimValue(claimValue);

            Assert.True(result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task HasClaimValue_WithoutMatchingValue_ShouldReturnFalse()
        {
            ClaimsPrincipal user = CreateClaimsPrincipal(new Claim("Permission", "ManageElection"));

            bool result = user.HasClaimValue("DeleteElection");

            Assert.False(result);

            return Task.CompletedTask;
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("admin")]
        [InlineData("ADMIN")]
        public Task HasRole_WithMatchingRole_ShouldReturnTrue(string role)
        {
            ClaimsPrincipal user = CreateClaimsPrincipal(new Claim(ClaimTypes.Role, "Admin"));

            bool result = user.HasRole(role);

            Assert.True(result);

            return Task.CompletedTask;
        }

        [Fact]
        public Task HasRole_WithoutMatchingRole_ShouldReturnFalse()
        {
            ClaimsPrincipal user = CreateClaimsPrincipal(new Claim(ClaimTypes.Role, "Admin"));

            bool result = user.HasRole("Student");

            Assert.False(result);

            return Task.CompletedTask;
        }

        private static ClaimsPrincipal CreateClaimsPrincipal(params Claim[] claims)
        {
            ClaimsIdentity identity = new(claims, "TestAuthentication");

            return new ClaimsPrincipal(identity);
        }
    }
}