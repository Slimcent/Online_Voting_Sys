using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<Result<string>> CreateDepartment(CreateDepartmentRequest request);

        Task<Result<PagedResponse<DepartmentResponse>>> GetDepartments(DepartmentRequestParameters parameters);

        Task<Result<DepartmentResponse>> GetDepartment(long id);

        Task<Result<IEnumerable<DepartmentResponse>>> GetDepartmentsByFacultyId(long facultyId);

        Task<Result<PagedResponse<DepartmentResponse>>> GetDepartmentsByFacultyId(long facultyId, DepartmentRequestParameters parameters);

        Task<Result<string>> UpdateDepartment(long id, CreateDepartmentRequest request);

        Task<Result<string>> ToggleDepartmentActivation(long id);

        Task<Result<string>> DeleteDepartment(long id);
    }
}