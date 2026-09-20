using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Tests.UnitTests.Models.Dtos.Request
{
    public class UploadStudentRequestTests
    {
        [Fact]
        public void Constructor_ShouldInitializeRequiredFields()
        {
            UploadStudentRequest request = new()
            {
                File = null!
            };

            Assert.Equal(
                new[]
                {
                    "RegNumber",
                    "FirstName",
                    "LastName",
                    "Email",
                    "PhoneNumber",
                    "Gender",
                    "Department"
                },
                request.RequiredColumns);
        }

        [Fact]
        public void Constructor_ShouldInitializeIgnoreFields()
        {
            UploadStudentRequest request = new()
            {
                File = null!
            };
        }
    }
}