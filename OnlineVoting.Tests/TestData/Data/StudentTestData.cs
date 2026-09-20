using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class StudentTestData
    {
        public static Student CreateInactiveStudentWithUser(string userId)
        {
            return new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = new User
                {
                    Id = userId
                },
                Active = false
            };
        }

        public static Student CreateActiveStudentWithUser(string userId)
        {
            return new Student
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                User = new User
                {
                    Id = userId
                },
                Active = true
            };
        }

        public static Student CreateInactiveStudentWithoutUser()
        {
            return new Student
            {
                Id = Guid.NewGuid(),
                Active = false
            };
        }
    }
}