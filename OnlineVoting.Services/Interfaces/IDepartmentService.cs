using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<Result<string>> CreateDepartment(CreateDepartmentRequest request);
    }
}