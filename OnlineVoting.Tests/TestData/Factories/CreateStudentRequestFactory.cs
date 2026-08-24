using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Tests.TestData.Constants;

namespace OnlineVoting.Tests.TestData.Factories
{
    public static class CreateStudentRequestFactory
    {
        public static CreateStudentRequest CreateValid()
        {
            return new CreateStudentRequest
            {
                FirstName = TestValues.ValidName,
                LastName = "Obinna",
                Email = TestValues.ValidEmail,
                PhoneNumber = TestValues.ValidPhoneNumber,
                GenderId = TestValues.ValidGenderId,
                UserType = TestValues.ValidUserType,
                Role = TestValues.ValidRole,
                DepartmentId = 1
            };
        }
    }
}