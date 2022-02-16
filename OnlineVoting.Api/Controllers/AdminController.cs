using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IUserService _userService;

        public AdminController(IStudentService studentService, IUserService userService)
        {
            _studentService = studentService;
            _userService = userService;
        }

        [HttpPost("createuser")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequestDto request)
        {

            var user = await _userService.CreateUser(request);

            return Ok(user);
        }
    }
}
