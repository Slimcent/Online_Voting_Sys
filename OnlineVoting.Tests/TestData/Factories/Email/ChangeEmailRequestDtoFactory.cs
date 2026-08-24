using OnlineVoting.Models.Dtos.Request.Email;

namespace OnlineVoting.Tests.TestData.Factories.Email
{
    public static class ChangeEmailRequestDtoFactory
    {
        public static ChangeEmailRequestDto CreateValid()
        {
            return new ChangeEmailRequestDto
            {
                NewEmail = "encoded-new-email",
                Token = "encoded-token"
            };
        }
    }
}