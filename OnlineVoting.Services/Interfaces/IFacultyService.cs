using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IFacultyService
    {
        Task<Result<string>> CreateFaculty(CreateFacultyRequest request);

        Task<Result<PagedResponse<FacultyResponse>>> GetFaculties(FacultyRequestParameters parameters);

        Task<Result<FacultyResponse>> GetFaculty(long id);

        Task<Result<PagedResponse<FacultyResponse>>> GetFacultiesWithDepartments(FacultyRequestParameters parameters);

        Task<Result<FacultyResponse>> GetFacultyWithDepartments(long id);

        Task<Result<string>> UpdateFaculty(long id, CreateWithNameRequest request);

        Task<Result<string>> ToggleFacultyActivation(long id);

        Task<Result<string>> DeleteFaculty(long id);
    }
}