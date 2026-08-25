using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Policy = "Authorization")]
    public class FacultyController : BaseController
    {
        private readonly IFacultyService _facultyService;

        public FacultyController(IFacultyService facultyService)
        {
            _facultyService = facultyService;
        }

        [HttpPost("create-faculty", Name = "Create-Faculty")]
        [ApiDocumentation(FacultyDocumentationKeys.CreateFaculty)]
        public async Task<IActionResult> CreateFaculty([FromBody] CreateFacultyRequest request)
        {
            Result<string> result = await _facultyService.CreateFaculty(request);

            return result.ToActionResult(this);
        }

        [HttpGet("faculties", Name = "Get-Faculties")]
        public async Task<IActionResult> GetFaculties([FromQuery] FacultyRequestParameters parameters)
        {
            Result<PagedResponse<FacultyResponse>> result = await _facultyService.GetFaculties(parameters);

            return result.ToActionResult(this);
        }

        [HttpGet("{id:long}", Name = "Get-Faculty")]
        public async Task<IActionResult> GetFaculty(long id)
        {
            Result<FacultyResponse> result = await _facultyService.GetFaculty(id);

            return result.ToActionResult(this);
        }

        [HttpPut("{id:long}", Name = "Update-Faculty")]
        public async Task<IActionResult> UpdateFaculty(long id, [FromBody] CreateWithNameRequest request)
        {
            Result<string> result = await _facultyService.UpdateFaculty(id, request);

            return result.ToActionResult(this);
        }

        [HttpPatch("{id:long}/faculty-activation", Name = "Faculty-Activation")]
        public async Task<IActionResult> ToggleFacultyActivation(long id)
        {
            Result<string> result = await _facultyService.ToggleFacultyActivation(id);

            return result.ToActionResult(this);
        }

        [HttpDelete("{id:long}", Name = "Delete-Faculty")]
        public async Task<IActionResult> DeleteFaculty(long id)
        {
            Result<string> result = await _facultyService.DeleteFaculty(id);

            return result.ToActionResult(this);
        }

        [HttpGet("faculties-with-departments", Name = "Get-Faculties-With-Departments")]
        public async Task<IActionResult> GetFacultiesWithDepartments([FromQuery] FacultyRequestParameters parameters)
        {
            Result<PagedResponse<FacultyResponse>> result = await _facultyService.GetFacultiesWithDepartments(parameters);

            return result.ToActionResult(this);
        }

        [HttpGet("{id:long}/faculty-with-departments", Name = "Get-Faculty-With-Departments")]
        public async Task<IActionResult> GetFacultyWithDepartments(long id)
        {
            Result<FacultyResponse> result = await _facultyService.GetFacultyWithDepartments(id);

            return result.ToActionResult(this);
        }
    }
}