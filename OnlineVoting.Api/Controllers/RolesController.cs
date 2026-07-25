using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Policy = "Authorization")]
    public class RolesController : BaseController
    {
        private readonly IRolesService _roleService;

        public RolesController(IRolesService roleService) =>
            _roleService = roleService;

        [HttpGet("all-roles", Name = "All-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllRoles)]
        public async Task<IActionResult> GetAllRoles()
        {
            IEnumerable<RoleResponse> roles = await _roleService.GetAllRoles();

            return Ok(roles);
        }

        [HttpGet("all-active-roles", Name = "All-Active-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllActiveRoles)]
        public async Task<IActionResult> GetAllActiveRoles()
        {
            IEnumerable<RoleResponse> roles = await _roleService.GetAllActiveRoles();

            return Ok(roles);
        }

        [HttpGet("all-deactivated-roles",  Name = "All-Deactivated-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllDeactivatedRoles)]
        public async Task<IActionResult> GetAllDeactivatedRoles()
        {
            IEnumerable<RoleResponse> roles = await _roleService.GetAllDeactivatedRoles();

            return Ok(roles);
        }

        [HttpGet("all-paged-roles", Name = "All-Paged-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllPagedRoles)]
        public async Task<IActionResult> AllPagedRoles([FromQuery] RoleRequest request)
        {
            PagedResponse<RoleResponse> roles = await _roleService.AllRoles(request);

            return Ok(roles);
        }

        [HttpGet("all-paged-active-roles", Name = "All-Paged-Active-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllPagedActiveRoles)]
        public async Task<IActionResult> AllPagedActiveRoles([FromQuery] RoleRequest request)
        {
            PagedResponse<RoleResponse> roles = await _roleService.AllActiveRoles(request);

            return Ok(roles);
        }

        [HttpGet("all-paged-deactivated-roles", Name = "All-Paged-Deactivated-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllPagedDeactivatedRoles)]
        public async Task<IActionResult> AllPagedDeactivatedRoles([FromQuery] RoleRequest request)
        {
            PagedResponse<RoleResponse> roles = await _roleService.AllDeactivatedRoles(request);

            return Ok(roles);
        }

        [HttpGet("user-roles", Name = "User-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetUserRoles)]
        public async Task<IActionResult> GetUserRoles([FromQuery] string userName)
        {
            var roles = await _roleService.GetUserRoles(userName);

            return Ok(roles);
        }

        [HttpPost("create-role", Name = "Create-Role")]
        [ApiDocumentation(RoleDocumentationKeys.CreateRole)]
        public async Task<IActionResult> CreateRole([FromQuery] CreateRoleRequest request)
        {
            var role = await _roleService.CreateRole(request);

            return Ok(role);
        }

        [HttpPut("edit-role", Name = "Edit-Role")]
        [ApiDocumentation(RoleDocumentationKeys.EditRole)]
        public async Task<IActionResult> EditRole([FromQuery] string id, [FromBody] CreateRoleRequest request)
        {
            string role = await _roleService.EditRole(id, request);

            return Ok(role);
        }

        [HttpPost("add-user-to-role", Name = "Add-User-To-Role")]
        [ApiDocumentation(RoleDocumentationKeys.AddUserToRole)]
        public async Task<IActionResult> AddUserToRole([FromQuery] AddUserToRoleRequest request)
        {
            var user = await _roleService.AddUserToRole(request);

            return Ok(user);
        }

        [HttpPost("remove-user-from-role", Name = "Remove-User-From-Role")]
        [ApiDocumentation(RoleDocumentationKeys.RemoveUserFromRole)]
        public async Task<IActionResult> RemoveUserFromRole([FromQuery] AddUserToRoleRequest request)
        {
            var user =
                await _roleService.RemoveUserFromRole(request);

            return Ok(user);
        }

        [HttpPut("toggle-role-status", Name = "Toggle-Role-Status")]
        [ApiDocumentation(RoleDocumentationKeys.ToggleRoleStatus)]
        public async Task<IActionResult> ToggleRoleStatus([FromQuery] string id)
        {
            string role = await _roleService.ToggleRoleStatus(id);

            return Ok(role);
        }

        [HttpDelete("delete-role-by-id", Name = "Delete-Role_by-Id")]
        [ApiDocumentation(RoleDocumentationKeys.DeleteRoleById)]
        public async Task<IActionResult> DeleteUserRole([FromQuery] string id)
        {
            string role = await _roleService.DeleteUserRole(id);

            return Ok(role);
        }

        [HttpDelete("delete-role-by-name", Name = "Delete-Role_by-Name")]
        [ApiDocumentation(RoleDocumentationKeys.DeleteRoleByName)]
        public async Task<IActionResult> DeleteRole([FromBody] CreateRoleRequest request)
        {
            string role = await _roleService.DeleteRole(request);

            return Ok(role);
        }
    }
}