using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class CreateRoleRequestFactory
    {
        public static CreateRoleRequest CreateValid()
        {
            return new CreateRoleRequest
            {
                Name = "administrator"
            };
        }
    }
}