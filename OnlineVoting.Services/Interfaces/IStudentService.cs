using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.GlobalMessage;


namespace OnlineVoting.Services.Interfaces
{
    public interface IStudentService
    {
        Task<Response> CreateStudent(CreateStudentRequest model);
        Task<Response> CreateContestant(string regNo, string position);
        Task<Response> Vote(VoteRequest request);
        Task<String> UploadStudents(UploadStudentRequest model);
        Task<FileStreamDto> DownloadStudentsList();
    }
}