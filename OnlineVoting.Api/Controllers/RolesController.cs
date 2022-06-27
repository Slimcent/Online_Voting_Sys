using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRolesService _roleService;
        public RolesController(IRolesService roleService) => _roleService = roleService;


        [HttpGet("all-roles", Name = "All-Roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            IEnumerable<RoleResponseDto> roles = await _roleService.GetAllRoles();

            return Ok(roles);
        }

        [HttpGet("all-active-roles", Name = "All-Active-Roles")]
        public async Task<IActionResult> GetAllActiveRoles()
        {
            IEnumerable<RoleResponseDto> roles = await _roleService.GetAllActiveRoles();

            return Ok(roles);
        }

        [HttpGet("all-deactivated-roles", Name = "All-Deactivated-Roles")]
        public async Task<IActionResult> GetAllDeactivatedRoles()
        {
            IEnumerable<RoleResponseDto> roles = await _roleService.GetAllDeactivatedRoles();

            return Ok(roles);
        }

        [HttpGet("all-paged-roles", Name = "All-Paged-Roles")]
        public async Task<IActionResult> AllPagedRoles([FromQuery] RoleRequestDto request)
        {
            PagedResponse<RoleResponseDto> roles = await _roleService.AllRoles(request);

            return Ok(roles);
        }

        [HttpGet("all-paged-active-roles", Name = "All-Paged-Active-Roles")]
        public async Task<IActionResult> AllPagedActiveRoles([FromQuery] RoleRequestDto request)
        {
            PagedResponse<RoleResponseDto> roles = await _roleService.AllActiveRoles(request);

            return Ok(roles);
        }

        [HttpGet("all-paged-deactivated-roles", Name = "All-Paged-Deactivated-Roles")]
        public async Task<IActionResult> AllPagedDeactivatedRoles([FromQuery] RoleRequestDto request)
        {
            PagedResponse<RoleResponseDto> roles = await _roleService.AllDeactivatedRoles(request);

            return Ok(roles);
        }

        [HttpGet("user-roles", Name = "User-Roles")]
        public async Task<IActionResult> GetUserRoles([FromQuery] string userName)
        {
            var roles = await _roleService.GetUserRoles(userName);

            return Ok(roles);
        }

        [HttpPost("create-role", Name = "Create-Role")]
        public async Task<IActionResult> CreateRole([FromQuery] RoleDto request)
        {
            var role = await _roleService.CreateRole(request);

            return Ok(role);
        }

        [HttpPut("edit-role", Name = "Edit-Role")]
        public async Task<IActionResult> EditRole(string id, RoleDto request)
        {
            string role = await _roleService.EditRole(id, request);

            return Ok(role);
        }

        [HttpPost("add-user-to-role", Name = "Add-User-To-Role")]
        public async Task<IActionResult> AddUserToRole([FromQuery] AddUserToRoleDto request)
        {
            var user = await _roleService.AddUserToRole(request);

            return Ok(user);
        }


        [HttpPost("remove-user-from-role", Name = "Remove-User-From-Role")]
        public async Task<IActionResult> RemoveUserFromRole([FromQuery] AddUserToRoleDto request)
        {
            var user = await _roleService.RemoveUserFromRole(request);

            return Ok(user);
        }

        [HttpPut("toggle-role-status", Name = "Toggle-Role-Status")]
        public async Task<IActionResult> ToggleRoleStatus(string id)
        {
            string role = await _roleService.ToggleRoleStatus(id);

            return Ok(role);
        }

        [HttpDelete("delete-role-by-id", Name = "Delete-Role_by-Id")]
        public async Task<IActionResult> DeleteUserRole([FromQuery] string id)
        {
            string role = await _roleService.DeleteUserRole(id);

            return Ok(role);
        }

        [HttpDelete("delete-role-by-name", Name = "Delete-Role_by-Name")]
        public async Task<IActionResult> DeleteRole(RoleDto request)
        {
            string role = await _roleService.DeleteRole(request);

            return Ok(role);
        }
    }
}
