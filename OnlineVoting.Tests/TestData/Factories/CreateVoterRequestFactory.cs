using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class RegisterVoterRequestFactory
    {
        public static RegisterVoterRequest Create(string regNumber, string electionId)
        {
            return new RegisterVoterRequest
            {
                RegNumber = regNumber,
                ElectionId = electionId
            };
        }
    }
}