using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class CreateDepartmentRequestFactory
    {
        public static CreateDepartmentRequest CreateValidSingle()
        {
            return new CreateDepartmentRequest
            {
                Name = TestValues.ValidName,
                FacultyId = 1
            };
        }

        public static CreateDepartmentRequest CreateValidMultiple()
        {
            return new CreateDepartmentRequest
            {
                Names = new List<string>
                {
                    TestValues.ValidName,
                    "Electrical Engineering"
                },
                FacultyId = 1
            };
        }
    }
}