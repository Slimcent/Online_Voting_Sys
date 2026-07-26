using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
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
            Result<IEnumerable<RoleResponse>> result = await _roleService.GetAllRoles();

            return result.ToActionResult(this);
        }

        [HttpGet("all-active-roles", Name = "All-Active-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllActiveRoles)]
        public async Task<IActionResult> GetAllActiveRoles()
        {
            Result<IEnumerable<RoleResponse>> result = await _roleService.GetAllActiveRoles();

            return result.ToActionResult(this);
        }

        [HttpGet("all-deactivated-roles", Name = "All-Deactivated-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllDeactivatedRoles)]
        public async Task<IActionResult> GetAllDeactivatedRoles()
        {
            Result<IEnumerable<RoleResponse>> result = await _roleService.GetAllDeactivatedRoles();

            return result.ToActionResult(this);
        }

        [HttpGet("all-paged-roles", Name = "All-Paged-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllPagedRoles)]
        public async Task<IActionResult> AllPagedRoles([FromQuery] RoleRequest request)
        {
            Result<PagedResponse<RoleResponse>> result = await _roleService.AllRoles(request);

            return result.ToActionResult(this);
        }

        [HttpGet("all-paged-active-roles", Name = "All-Paged-Active-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllPagedActiveRoles)]
        public async Task<IActionResult> AllPagedActiveRoles([FromQuery] RoleRequest request)
        {
            Result<PagedResponse<RoleResponse>> result = await _roleService.AllActiveRoles(request);

            return result.ToActionResult(this);
        }

        [HttpGet("all-paged-deactivated-roles", Name = "All-Paged-Deactivated-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetAllPagedDeactivatedRoles)]
        public async Task<IActionResult> AllPagedDeactivatedRoles([FromQuery] RoleRequest request)
        {
            Result<PagedResponse<RoleResponse>> result = await _roleService.AllDeactivatedRoles(request);

            return result.ToActionResult(this);
        }

        [HttpGet("user-roles", Name = "User-Roles")]
        [ApiDocumentation(RoleDocumentationKeys.GetUserRoles)]
        public async Task<IActionResult> GetUserRoles([FromQuery] string userName)
        {
            Result<IList<string>> result = await _roleService.GetUserRoles(userName);

            return result.ToActionResult(this);
        }

        [HttpPost("create-role", Name = "Create-Role")]
        [ApiDocumentation(RoleDocumentationKeys.CreateRole)]
        public async Task<IActionResult> CreateRole([FromQuery] CreateRoleRequest request)
        {
            Result<string> result = await _roleService.CreateRole(request);

            return result.ToActionResult(this);
        }

        [HttpPut("edit-role", Name = "Edit-Role")]
        [ApiDocumentation(RoleDocumentationKeys.EditRole)]
        public async Task<IActionResult> EditRole([FromQuery] string id, [FromBody] CreateRoleRequest request)
        {
            Result<string> result = await _roleService.EditRole(id, request);

            return result.ToActionResult(this);
        }

        [HttpPost("add-user-to-role", Name = "Add-User-To-Role")]
        [ApiDocumentation(RoleDocumentationKeys.AddUserToRole)]
        public async Task<IActionResult> AddUserToRole([FromQuery] AddUserToRoleRequest request)
        {
            Result<string> result = await _roleService.AddUserToRole(request);

            return result.ToActionResult(this);
        }

        [HttpPost("remove-user-from-role", Name = "Remove-User-From-Role")]
        [ApiDocumentation(RoleDocumentationKeys.RemoveUserFromRole)]
        public async Task<IActionResult> RemoveUserFromRole([FromQuery] AddUserToRoleRequest request)
        {
            Result<string> result = await _roleService.RemoveUserFromRole(request);

            return result.ToActionResult(this);
        }

        [HttpPut("toggle-role-status", Name = "Toggle-Role-Status")]
        [ApiDocumentation(RoleDocumentationKeys.ToggleRoleStatus)]
        public async Task<IActionResult> ToggleRoleStatus([FromQuery] string id)
        {
            Result<string> result = await _roleService.ToggleRoleStatus(id);

            return result.ToActionResult(this);
        }

        [HttpDelete("delete-role-by-id", Name = "Delete-Role_by-Id")]
        [ApiDocumentation(RoleDocumentationKeys.DeleteRoleById)]
        public async Task<IActionResult> DeleteUserRole([FromQuery] string id)
        {
            Result<string> result = await _roleService.DeleteUserRole(id);

            return result.ToActionResult(this);
        }

        [HttpDelete("delete-role-by-name", Name = "Delete-Role_by-Name")]
        [ApiDocumentation(RoleDocumentationKeys.DeleteRoleByName)]
        public async Task<IActionResult> DeleteRole([FromBody] CreateRoleRequest request)
        {
            Result<string> result = await _roleService.DeleteRole(request);

            return result.ToActionResult(this);
        }
    }
}