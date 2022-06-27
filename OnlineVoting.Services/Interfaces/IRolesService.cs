using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Services.Interfaces
{
    public interface IRolesService
    {
        Task<string> CreateRole(RoleDto request);
        Task<string> EditRole(string id, RoleDto request);
        Task<string> DeleteRole(RoleDto request);
        Task<string> AddUserToRole(AddUserToRoleDto request);
        Task<string> RemoveUserFromRole(AddUserToRoleDto request);
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
