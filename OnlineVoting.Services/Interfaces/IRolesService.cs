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
        Task<IEnumerable<RoleResponseDto>> GetAllRoles();
        Task<IEnumerable<RoleResponseDto>> GetAllActiveRoles();
        Task<IEnumerable<RoleResponseDto>> GetAllDeactivatedRoles();
        Task<PagedResponse<RoleResponseDto>> AllRoles(RoleRequestDto request);
        Task<PagedResponse<RoleResponseDto>> AllActiveRoles(RoleRequestDto request);
        Task<PagedResponse<RoleResponseDto>> AllDeactivatedRoles(RoleRequestDto request);
        Task<string> DeleteUserRole(string Id);
    }
}