using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories.Email
{
    public static class ResetPasswordRequestFactory
    {
        public static ResetPasswordRequest CreateValid()
        {
            return new ResetPasswordRequest
            {
                Email = "encoded-email",
                ResetPasswordToken = "encoded-reset-password-token",
                NewPassword = TestValues.ValidPassword
            };
        }
    }
}