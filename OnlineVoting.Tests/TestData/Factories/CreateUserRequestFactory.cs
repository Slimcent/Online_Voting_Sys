using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class CreateUserRequestFactory
    {
        public static CreateUserRequest CreateValid()
        {
            return new CreateUserRequest
            {
                FirstName = TestValues.ValidName,
                LastName = "Obinna",
                Email = TestValues.ValidEmail,
                PhoneNumber = TestValues.ValidPhoneNumber,
                GenderId = TestValues.ValidGenderId,
                UserType = TestValues.ValidUserType,
                Role = TestValues.ValidRole
            };
        }
    }
}