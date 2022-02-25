using Microsoft.AspNetCore.Http;
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
        private readonly IUserService _userService;

        public ClaimsController(IUserService userService) => _userService = userService;

        [HttpPost("addusertoclaims")]
        public async Task<IActionResult> AddUserToClaims(UserClaimsRequestDto request)
        {
            var user = await _userService.CreateUserClaims(request);

            return Ok(user);
        }

        [HttpPost("deleteclaim")]
        public async Task<IActionResult> DeleteClaim(UserClaimsRequestDto request)
        {
            var user = await _userService.DeleteClaims(request);

            return Ok(user);
        }

        [HttpPost("editclaim")]
        public async Task<IActionResult> EditClaim(EditUserClaimsDto editUserClaims)
        {
            var user = await _userService.EditUserClaims(editUserClaims);

            return Ok(user);
        }

        [HttpGet("getuserclaims")]
        public async Task<IActionResult> GetUserClaims(string email)
        {
            var userClaims = await _userService.GetUserClaims(email);

            if (userClaims.Any())
                return Ok(userClaims);

            return BadRequest(new ErrorResponse { Message = $"No Claims found for user {email}" });
        }

    }
}
