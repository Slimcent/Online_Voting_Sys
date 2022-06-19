using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.GlobalMessage;


namespace OnlineVoting.Services.Interfaces
{
    public interface IStudentService
    {
        Task<Response> CreateStudent(StudentCreateRequestDto model);
        Task<Response> CreateContestant(string regNo, string position);
        Task<Response> Vote(VoteRequestDto request);
        Task<String> UploadStudents(UploadStudentRequestDto model);
        Task<FileStreamDto> DownloadStudentsList();
    }
}
