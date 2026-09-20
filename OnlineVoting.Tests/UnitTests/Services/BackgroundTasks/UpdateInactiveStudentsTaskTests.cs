using Moq;
using OnlineVoting.Services.BackgroundTasks;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Tests.UnitTests.Services.BackgroundTasks
{
    public class UpdateInactiveStudentsTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldUpdateInactiveStudents()
        {
            Mock<IStudentService> studentService = new();

            UpdateInactiveStudentsTask task = new(studentService.Object);

            await task.Execute();

            studentService.Verify(service => service.UpdateInactiveStudents(), Times.Once);
        }


    }
}