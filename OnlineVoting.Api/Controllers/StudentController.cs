using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
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

        [HttpGet("download-students-excel-template", Name = "Download-Students-Excel-Template")]
        public async Task<IActionResult> DownloadCoursesSampleSheet()
        {
            FileStreamDto excelSheet = await _studentService.DownloadStudentsList();

            return File(excelSheet.FileStream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelSheet.FileName);
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
