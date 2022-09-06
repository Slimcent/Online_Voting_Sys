using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/claims")]
    [ApiController]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimsService _claimsService;

        public ClaimsController(IClaimsService claimsService) => _claimsService = claimsService;

        [HttpPost("addusertoclaims")]
        public async Task<IActionResult> AddUserToClaims(string email, string claimType, string claimValue)
        {
            var user = await _claimsService.CreateUserClaims(email, claimType, claimValue);

            return Ok(user);
        }

        [HttpPost("deleteclaim")]
        public async Task<IActionResult> DeleteClaim(UserClaimsRequestDto request)
        {
            var user = await _claimsService.DeleteClaims(request);

            return Ok(user);
        }

        [HttpPost("editclaim")]
        public async Task<IActionResult> EditClaim(EditUserClaimsDto editUserClaims)
        {
            var user = await _claimsService.EditUserClaims(editUserClaims);

            return Ok(user);
        }

        [HttpGet("userclaims")]
        public async Task<IActionResult> GetUserClaims(string email)
        {
            var userClaims = await _claimsService.GetUserClaims(email);

            if (userClaims.Any())
                return Ok(userClaims);

            return BadRequest(new ErrorResponse { Message = $"No Claims found for user {email}" });
        }

    }
}
