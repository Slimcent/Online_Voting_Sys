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

        [HttpPost("createstudent", Name = "Create-Students")]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateRequestDto request)
        {
            var student = await _studentService.CreateStudent(request);

            return Ok(student);
        }

        
        [HttpPost("UploadStudents", Name = "Upload-Students")]
        public async Task<IActionResult> UploadLecturers([FromForm] UploadStudentRequestDto students)
        {
            string res = await _studentService.UploadStudents(students);

            return Ok(res);
        }

        [HttpPost("create-contestant", Name = "Create-Contestants")]
        public async Task<IActionResult> CreateContestant(string regNo, string position)
        {
            var contestant = await _studentService.CreateContestant(regNo, position);

            return Ok(contestant);
        }
    }
}
