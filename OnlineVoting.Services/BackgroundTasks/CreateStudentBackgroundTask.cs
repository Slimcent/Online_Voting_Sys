using OnlineVoting.BackgroundTasks.Interfaces;
using OnlineVoting.Models.Dtos.Request.BackgroundRequests;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Services.BackgroundTasks
{
    public class CreateStudentBackgroundTask : IBackgroundTask<ProcessStudentUploadRequest>
    {
        private readonly IStudentService _studentService;

        public CreateStudentBackgroundTask(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public async Task ExecuteAsync(ProcessStudentUploadRequest request)
        {
            Result<Response> result = await _studentService.CreateStudent(request.Student);

            if (!result.IsSuccess)
                throw new InvalidOperationException($"Unable to create student {request.Student.Email}.");
        }
    }
}