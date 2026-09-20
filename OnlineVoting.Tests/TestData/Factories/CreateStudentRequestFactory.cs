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
                RegNumber = TestValues.ValidRegistrationNumber,
                PhoneNumber = TestValues.ValidPhoneNumber,
                GenderId = TestValues.ValidGenderId,
                UserTypeId = TestValues.ValidUserType,
                RoleId = TestValues.ValidRoleId,
                DepartmentId = 1
            };
        }
    }
}