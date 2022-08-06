using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login", Name = "Login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            LoggedInUserDto user = await _userService.UserLogin(loginDto);

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("verify-user", Name = "Verify-User")]
        public async Task<IActionResult> VerifyUser(VerifyAccountRequestDto request)
        {
            return Ok(await _userService.VerifyUser(request));
        }
    }
}
