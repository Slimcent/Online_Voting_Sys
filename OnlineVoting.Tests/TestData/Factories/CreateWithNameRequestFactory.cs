using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class CreateWithNameRequestFactory
    {
        public static CreateWithNameRequest CreateValid()
        {
            return new CreateWithNameRequest
            {
                Name = TestValues.ValidName
            };
        }
    }
}