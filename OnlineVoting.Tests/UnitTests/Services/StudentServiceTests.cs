using Moq;
using OnlineVoting.Models.Entities;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class StudentServiceTests
    {
        [Fact]
        public async Task UpdateInactiveStudents_WithMatchingStudents_ShouldActivateStudents()
        {
            using StudentServiceFactory factory = new();

            Student firstInactiveStudent = StudentTestData.CreateInactiveStudentWithUser("user-1");
            Student secondInactiveStudent = StudentTestData.CreateInactiveStudentWithUser("user-2");
            Student activeStudent = StudentTestData.CreateActiveStudentWithUser("user-3");
            Student studentWithoutUser = StudentTestData.CreateInactiveStudentWithoutUser();

            await factory.AddStudents(firstInactiveStudent, secondInactiveStudent, activeStudent, studentWithoutUser);

            int result = await factory.Service.UpdateInactiveStudents();

            Assert.Equal(2, result);
            Assert.True(firstInactiveStudent.Active);
            Assert.True(secondInactiveStudent.Active);
            Assert.True(activeStudent.Active);
            Assert.False(studentWithoutUser.Active);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateInactiveStudents_WithNoMatchingStudents_ShouldReturnZero()
        {
            using StudentServiceFactory factory = new();

            Student activeStudent = StudentTestData.CreateActiveStudentWithUser("user-1");
            Student inactiveStudentWithoutUser = StudentTestData.CreateInactiveStudentWithoutUser();

            await factory.AddStudents(activeStudent, inactiveStudentWithoutUser);

            int result = await factory.Service.UpdateInactiveStudents();

            Assert.Equal(0, result);
            Assert.True(activeStudent.Active);
            Assert.False(inactiveStudentWithoutUser.Active);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(), Times.Never);
        }
    }
}