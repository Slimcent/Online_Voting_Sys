using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<string> CreateDepartment(DeptCreateDto request);
    }
}
