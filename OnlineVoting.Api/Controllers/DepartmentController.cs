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
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpPost("create-department", Name = "Create-Department")]
        [ApiDocumentation(DepartmentDocumentationKeys.CreateDepartment)]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
        {
            Result<string> result = await _departmentService.CreateDepartment(request);

            return result.ToActionResult(this);
        }

        [HttpGet("departments", Name = "Get-Departments")]
        public async Task<IActionResult> GetDepartments([FromQuery] DepartmentRequestParameters parameters)
        {
            Result<PagedResponse<DepartmentResponse>> result = await _departmentService.GetDepartments(parameters);

            return result.ToActionResult(this);
        }

        [HttpGet("{id:long}", Name = "Get-Department")]
        public async Task<IActionResult> GetDepartment(long id)
        {
            Result<DepartmentResponse> result = await _departmentService.GetDepartment(id);

            return result.ToActionResult(this);
        }

        [HttpGet("faculty/{facultyId:long}", Name = "Get-Departments-By-Faculty")]
        public async Task<IActionResult> GetDepartmentsByFacultyId(long facultyId)
        {
            Result<IEnumerable<DepartmentResponse>> result = await _departmentService.GetDepartmentsByFacultyId(facultyId);

            return result.ToActionResult(this);
        }

        [HttpGet("faculty/{facultyId:long}/paged-departments", Name = "Get-Paged-Departments-By-Faculty")]
        public async Task<IActionResult> GetDepartmentsByFacultyId(long facultyId, [FromQuery] DepartmentRequestParameters parameters)
        {
            Result<PagedResponse<DepartmentResponse>> result = await _departmentService.GetDepartmentsByFacultyId(facultyId, parameters);

            return result.ToActionResult(this);
        }

        [HttpPut("{id:long}", Name = "Update-Department")]
        public async Task<IActionResult> UpdateDepartment(long id, [FromBody] CreateDepartmentRequest request)
        {
            Result<string> result = await _departmentService.UpdateDepartment(id, request);

            return result.ToActionResult(this);
        }

        [HttpPatch("{id:long}/department-activation", Name = "Department-Activation")]
        public async Task<IActionResult> ToggleDepartmentActivation(long id)
        {
            Result<string> result = await _departmentService.ToggleDepartmentActivation(id);

            return result.ToActionResult(this);
        }

        [HttpDelete("{id:long}", Name = "Delete-Department")]
        public async Task<IActionResult> DeleteDepartment(long id)
        {
            Result<string> result = await _departmentService.DeleteDepartment(id);

            return result.ToActionResult(this);
        }
    }
}