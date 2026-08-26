using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities.Configurations;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;

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
        [EnableRateLimiting(RateLimitPolicyNames.Authentication)]
        [ApiDocumentation(AuthDocumentationKeys.Auth.Login)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Result<LoggedInUserResponse> result = await _userService.UserLogin(request);

            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("verify-user", Name = "Verify-User")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.VerifyUser)]
        public async Task<IActionResult> VerifyUser([FromBody] VerifyAccountRequest request)
        {
            Result<string> result = await _userService.VerifyUser(request);

            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("send-reset-password-mail", Name = "Request-Password-Mail")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.SendResetPasswordMail)]
        public async Task<IActionResult> SendResetPasswordMail([FromQuery] string email)
        {
            Result<string> result = await _emailService.SendResetPasswordEmail(email);

            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("reset-password", Name = "Reset-Password")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.ResetPassword)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            Result<string> result = await _userService.ResetPassword(request);

            return result.ToActionResult(this);
        }

        [HttpPost("change-password", Name = "Change-Password")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.ChangePassword)]
        public async Task<IActionResult> ChangePassword([FromQuery] string userId, [FromBody] ChangePasswordRequest request)
        {
            Result<string> result = await _userService.ChangePassword(userId, request);

            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("update-recovery-email", Name = "Update-Recovery-Email")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.UpdateRecoveryEmail)]
        public async Task<IActionResult> UpdateRecoveryEmail([FromQuery] string userId, [FromQuery] string email)
        {
            Result<string> result = await _userService.UpdateRecoveryEmail(userId, email);

            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("send-change-email-mail", Name = "Send-Change-Email-Mail")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.SendChangeEmailMail)]
        public async Task<IActionResult> SendChangeEmailMail([FromBody] ChangeEmailRequest request)
        {
            Result<string> result = await _emailService.SendChangeEmail(request);

            return result.ToActionResult(this);
        }

        [AllowAnonymous]
        [HttpPost("change-email", Name = "Change-Email")]
        [ApiDocumentation(AuthDocumentationKeys.Auth.ChangeEmail)]
        public async Task<IActionResult> ChangeEmail([FromQuery] string userId, [FromBody] ChangeEmailRequestDto request)
        {
            Result<string> result = await _userService.ChangeEmail(userId, request);

            return result.ToActionResult(this);
        }
    }
}