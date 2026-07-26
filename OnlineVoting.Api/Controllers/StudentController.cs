using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Policy = "Authorization")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [ApiDocumentation(StudentDocumentationKeys.CreateStudent)]
        [HttpPost("createstudent", Name = "Create-Students")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
        {
            Result<Response> result = await _studentService.CreateStudent(request);

            return result.ToActionResult(this);
        }

        [ApiDocumentation(StudentDocumentationKeys.DownloadStudentsExcelTemplate)]
        [HttpGet("download-students-excel-template", Name = "Download-Students-Excel-Template")]
        public async Task<IActionResult> DownloadCoursesSampleSheet()
        {
            Models.Dtos.Response.FileStreamResponse excelSheet = await _studentService.DownloadStudentsList();

            return File(excelSheet.FileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelSheet.FileName);
        }

        [ApiDocumentation(StudentDocumentationKeys.UploadStudents)]
        [HttpPost("UploadStudents", Name = "Upload-Students")]
        public async Task<IActionResult> UploadLecturers([FromForm] UploadStudentRequest students)
        {
            Result<string> result = await _studentService.UploadStudents(students);

            return result.ToActionResult(this);
        }

        [HttpPost("create-contestant", Name = "Create-Contestants")]
        [ApiDocumentation(StudentDocumentationKeys.CreateContestant)]
        public async Task<IActionResult> CreateContestant([FromQuery] string regNo, [FromQuery] string position)
        {
            Result<Response> result = await _studentService.CreateContestant(regNo, position);

            return result.ToActionResult(this);
        }
    }
}