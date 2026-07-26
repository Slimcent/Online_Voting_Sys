using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Models.Results;
using OnlineVoting.Api.Extensions;

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
        public async Task<IActionResult> CreateFaculty([FromQuery] CreateWithNameRequest request)
        {
            Result<string> result = await _facultyService.CreateFaculty(request);

            return result.ToActionResult(this);
        }
    }
}