using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;

namespace OnlineVoting.Services.Interfaces
{
    public interface IRolesService
    {
        Task<string> CreateRole(RoleDto request);
        Task EditRole(string id, RoleDto request);
        Task<string> DeleteRole(RoleDto request);
        Task<string> AddUserToRole(AddUserToRoleDto request);
        Task<string> RemoveUserFromRole(AddUserToRoleDto request);
        Task<IList<string>> GetUserRoles(string userName);
    }
}
