using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories.Email
{
    public static class ChangeEmailRequestFactory
    {
        public static ChangeEmailRequest CreateValid()
        {
            return new ChangeEmailRequest
            {
                Email = TestValues.ValidEmail,
                NewEmail = "newuser@example.com",
                RecoveryEmail = "recovery@example.com"
            };
        }
    }
}