using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.GlobalMessage;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Policy = "Authorization")]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimsService _claimsService;

        public ClaimsController(IClaimsService claimsService) => _claimsService = claimsService;

        [HttpPost("addusertoclaims")]
        [ApiDocumentation(ClaimsDocumentationKeys.AddUserToClaims)]
        public async Task<IActionResult> AddUserToClaims([FromQuery] string email, [FromQuery] string claimType, [FromQuery] string claimValue)
        {
            var user = await _claimsService.CreateUserClaims(email, claimType, claimValue);

            return Ok(user);
        }

        [HttpPost("deleteclaim")]
        [ApiDocumentation(ClaimsDocumentationKeys.DeleteClaim)]
        public async Task<IActionResult> DeleteClaim([FromBody] UserClaimsRequest request)
        {
            var user = await _claimsService.DeleteClaims(request);

            return Ok(user);
        }

        [HttpPost("editclaim")]
        [ApiDocumentation(ClaimsDocumentationKeys.EditClaim)]
        public async Task<IActionResult> EditClaim([FromBody] EditUserClaimsRequest request)
        {
            var user = await _claimsService.EditUserClaims(request);

            return Ok(user);
        }

        [HttpGet("userclaims")]
        [ApiDocumentation(ClaimsDocumentationKeys.GetUserClaims)]
        public async Task<IActionResult> GetUserClaims([FromQuery] string email)
        {
            var userClaims = await _claimsService.GetUserClaims(email);

            if (userClaims.Any())
                return Ok(userClaims);

            return BadRequest(new ResponseError {Message = $"No Claims found for user {email}"});
        }
    }
}