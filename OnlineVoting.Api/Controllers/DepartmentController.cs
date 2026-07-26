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
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _deptServie;

        public DepartmentController(IDepartmentService deptServie)
        {
            _deptServie = deptServie;
        }


        [HttpPost("create-department", Name = "Create-Department")]
        [ApiDocumentation(DepartmentDocumentationKeys.CreateDepartment)]
        public async Task<IActionResult> CreateDepartment([FromQuery] CreateDepartmentRequest request)
        {
            Result<string> result = await _deptServie.CreateDepartment(request);

            return result.ToActionResult(this);
        }
    }
}