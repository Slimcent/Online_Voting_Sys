using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost("createstudent")]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateRequestDto request)
        {
            var student = await _studentService.CreateStudent(request);

            return Ok(student);
        }
    }
}
