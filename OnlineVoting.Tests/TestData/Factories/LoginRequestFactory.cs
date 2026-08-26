using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class LoginRequestFactory
    {
        public static LoginRequest CreateValid()
        {
            return new LoginRequest
            {
                Email = TestValues.ValidEmail,
                Password = TestValues.ValidPassword
            };
        }
    }
}