using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class ChangePasswordRequestFactory
    {
        public static ChangePasswordRequest CreateValid()
        {
            return new ChangePasswordRequest
            {
                CurrentPassword = TestValues.ValidPassword,
                NewPassword = TestValues.DifferentValidPassword
            };
        }
    }
}