using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;
using System.Security.Claims;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class ClaimsTestData
    {
        public static User CreateUser(string email = "user@example.com")
        {
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email
            };
        }

        public static UserClaimsRequest CreateRequest(string email = "user@example.com", string claimType = "Permission",
            string claimValue = "ManageElection", string? oldValue = null)
        {
            return new UserClaimsRequest
            {
                Email = email,
                ClaimType = claimType,
                ClaimValue = claimValue,
                OldValue = oldValue
            };
        }

        public static Claim CreateClaim(string claimType = "Permission", string claimValue = "ManageElection")
        {
            return new Claim(claimType, claimValue);
        }
    }
}