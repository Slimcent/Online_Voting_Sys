using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class CreateVoterRequestFactory
    {
        public static CreateVoterRequest CreateValid()
        {
            return new CreateVoterRequest
            {
                RegNumber = "CS/2025/001"
            };
        }
    }
}