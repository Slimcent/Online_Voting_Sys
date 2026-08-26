using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class AddUserToRoleRequestFactory
    {
        public static AddUserToRoleRequest CreateValid()
        {
            return new AddUserToRoleRequest
            {
                Name = TestValues.ValidName,
                Email = TestValues.ValidEmail
            };
        }
    }
}