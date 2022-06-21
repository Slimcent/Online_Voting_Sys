using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacultyController : ControllerBase
    {
        private readonly IFacultyService _facultyService;

        public FacultyController(IFacultyService facultyService)
        {
            _facultyService = facultyService;
        }

        [HttpPost("create-faculty", Name = "Create-Faculty")]
        public async Task<IActionResult> CreateFaculty([FromQuery] CreateFacultyDto model)
        {
            string faculty = await _facultyService.CreateFaculty(model);

            return Ok(faculty);
        }
    }
}
