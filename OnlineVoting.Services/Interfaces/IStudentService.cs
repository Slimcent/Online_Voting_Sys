using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IStudentService
    {
        Task<Result<Response>> CreateStudent(CreateStudentRequest request);
        Task<Result<Response>> CreateContestant(string regNo, string position);
        Task<Response> Vote(VoteRequest request);
        Task<Result<string>> UploadStudents(UploadStudentRequest request);
        Task<Models.Dtos.Response.FileStreamResponse> DownloadStudentsList();
    }
}