using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class UserClaimsRequestFactory
    {
        public static UserClaimsRequest CreateValid()
        {
            return new UserClaimsRequest
            {
                Email = TestValues.ValidEmail,
                ClaimType = TestValues.ValidClaimType,
                ClaimValue = TestValues.ValidClaimValue
            };
        }

        public static UserClaimsRequest CreateValidForEdit()
        {
            UserClaimsRequest request = CreateValid();
            request.OldValue = TestValues.ValidOldClaimValue;

            return request;
        }
    }
}