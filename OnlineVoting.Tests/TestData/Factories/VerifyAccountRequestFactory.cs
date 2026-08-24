using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class VerifyAccountRequestFactory
    {
        public static VerifyAccountRequest CreateValid()
        {
            return new VerifyAccountRequest
            {
                Email = TestValues.ValidEmail,
                EmailConfirmationToken = "email-confirmation-token",
                ResetPasswordToken = "password-reset-token",
                NewPassword = TestValues.ValidPassword
            };
        }
    }
}