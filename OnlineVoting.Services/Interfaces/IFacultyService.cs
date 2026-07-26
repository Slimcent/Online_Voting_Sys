using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IFacultyService
    {
        Task<Result<string>> CreateFaculty(CreateWithNameRequest request);
    }
}