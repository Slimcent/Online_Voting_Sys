using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IRolesService
    {
        Task<Result<string>> CreateRole(CreateRoleRequest request);
        Task<Result<string>> EditRole(string id, CreateRoleRequest request);
        Task<Result<string>> DeleteRole(CreateRoleRequest request);
        Task<Result<string>> AddUserToRole(AddUserToRoleRequest request);
        Task<Result<string>> RemoveUserFromRole(AddUserToRoleRequest request);
        Task<Result<IList<string>>> GetUserRoles(string userName);
        Task<Result<string>> ToggleRoleStatus(string roleId);
        Task<Result<IEnumerable<RoleResponse>>> GetAllRoles();
        Task<Result<IEnumerable<RoleResponse>>> GetAllActiveRoles();
        Task<Result<IEnumerable<RoleResponse>>> GetAllDeactivatedRoles();
        Task<Result<PagedResponse<RoleResponse>>> AllRoles(RoleRequest request);
        Task<Result<PagedResponse<RoleResponse>>> AllActiveRoles(RoleRequest request);
        Task<Result<PagedResponse<RoleResponse>>> AllDeactivatedRoles(RoleRequest request);
        Task<Result<string>> DeleteUserRole(string id);
    }
}