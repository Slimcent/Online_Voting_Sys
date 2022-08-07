using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public AuthController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
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

        [AllowAnonymous]
        [HttpPost("send-reset-password-mail", Name = "Request-Password-Mail")]
        public async Task<IActionResult> SendResetPasswordMail(string email)
        {
            return Ok(await _emailService.SendResetPasswordEmail(email));
        }

        [AllowAnonymous]
        [HttpPost("reset-password", Name = "Reset-Password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto request)
        {
            return Ok(await _userService.ResetPassword(request));
        }

        [HttpPost("change-password", Name = "Change-Password")]
        public async Task<IActionResult> ChangePassword(string userId, ChangePasswordRequestDto request)
        {
            return Ok(await _userService.ChangePassword(userId, request));
        }

        [HttpPost("update-recovery-email", Name = "Update-Recovery-Email")]
        public async Task<IActionResult> UpdateRecoveryEmail(string userId, string email)
        {
            await _userService.UpdateRecoveryEmail(userId, email);
            return Ok();
        }

        [HttpPost("send-change-email-mail", Name = "send-change-email-mail")]
        public async Task<IActionResult> SendResetEmail(ChangeEmailDto request)
        {
            return Ok(await _emailService.SendChangeEmail(request));
        }

        [AllowAnonymous]
        [HttpPost("change-email", Name = "Change-Email")]
        public async Task<IActionResult> ChangeEmail(string userId, ChangeEmailRequestDto request)
        {
            return Ok(await _userService.ChangeEmail(userId, request));
        }
    }
}
