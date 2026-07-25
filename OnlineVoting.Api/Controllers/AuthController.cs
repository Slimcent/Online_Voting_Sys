using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;

namespace OnlineVoting.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Policy = "Authorization")]
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public AuthController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }


        
        [AllowAnonymous]
        [HttpPost("login", Name = "Login")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.Login)]
        public async Task<ActionResult<LoggedInUserResponse>> Login([FromBody] LoginRequest request)
        {
            LoggedInUserResponse user = await _userService.UserLogin(request);

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("verify-user", Name = "Verify-User")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.VerifyUser)]
        public async Task<IActionResult> VerifyUser(VerifyAccountRequest request)
        {
            return Ok(await _userService.VerifyUser(request));
        }

        [AllowAnonymous]
        [HttpPost("send-reset-password-mail", Name = "Request-Password-Mail")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.SendResetPasswordMail)]
        public async Task<IActionResult> SendResetPasswordMail(string email)
        {
            return Ok(await _emailService.SendResetPasswordEmail(email));
        }

        [AllowAnonymous]
        [HttpPost("reset-password", Name = "Reset-Password")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.ResetPassword)]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            return Ok(await _userService.ResetPassword(request));
        }

        [HttpPost("change-password", Name = "Change-Password")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.ChangePassword)]
        public async Task<IActionResult> ChangePassword(string userId, ChangePasswordRequest request)
        {
            return Ok(await _userService.ChangePassword(userId, request));
        }

        [AllowAnonymous]
        [HttpPost("update-recovery-email", Name = "Update-Recovery-Email")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.UpdateRecoveryEmail)]
        public async Task<IActionResult> UpdateRecoveryEmail(string userId, string email)
        {
            await _userService.UpdateRecoveryEmail(userId, email);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("send-change-email-mail", Name = "send-change-email-mail")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.SendChangeEmailMail)]
        public async Task<IActionResult> SendResetEmail(ChangeEmailRequest request)
        {
            return Ok(await _emailService.SendChangeEmail(request));
        }

        [AllowAnonymous]
        [HttpPost("change-email", Name = "Change-Email")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.ChangeEmail)]
        public async Task<IActionResult> ChangeEmail(string userId, ChangeEmailRequestDto request)
        {
            return Ok(await _userService.ChangeEmail(userId, request));
        }
    }
}