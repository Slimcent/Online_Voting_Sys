using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Services.Interfaces
{
    public interface IRolesService
    {
        Task<string> CreateRole(CreateRoleRequest request);
        Task<string> EditRole(string id, CreateRoleRequest request);
        Task<string> DeleteRole(CreateRoleRequest request);
        Task<string> AddUserToRole(AddUserToRoleRequest request);
        Task<string> RemoveUserFromRole(AddUserToRoleRequest request);
        Task<IList<string>> GetUserRoles(string userName);
        Task<string> ToggleRoleStatus(string roleId);
        Task<IEnumerable<RoleResponse>> GetAllRoles();
        Task<IEnumerable<RoleResponse>> GetAllActiveRoles();
        Task<IEnumerable<RoleResponse>> GetAllDeactivatedRoles();
        Task<PagedResponse<RoleResponse>> AllRoles(RoleRequest request);
        Task<PagedResponse<RoleResponse>> AllActiveRoles(RoleRequest request);
        Task<PagedResponse<RoleResponse>> AllDeactivatedRoles(RoleRequest request);
        Task<string> DeleteUserRole(string Id);
    }
}