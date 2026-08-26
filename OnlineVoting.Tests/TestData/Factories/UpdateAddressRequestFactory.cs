using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class UpdateAddressRequestFactory
    {
        public static UpdateAddressRequest CreateValid()
        {
            return new UpdateAddressRequest
            {
                PlotNo = 12,
                StreetName = "Main Street",
                City = "Paderborn",
                State = "North Rhine",
                Nationality = "Nigerian"
            };
        }
    }
}