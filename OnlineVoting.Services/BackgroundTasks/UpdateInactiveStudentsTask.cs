using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Services.BackgroundTasks
{
    public class UpdateInactiveStudentsTask
    {
        private readonly IStudentService _studentService;

        public UpdateInactiveStudentsTask(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public async Task Execute()
        {
            await _studentService.UpdateInactiveStudents();
        }
    }
}