using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Results;
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
        public async Task<IActionResult> AddUserToClaims([FromBody] UserClaimsRequest request)
        {
            Result<UserClaimsResponse> result = await _claimsService.CreateUserClaims(request);

            return result.ToActionResult(this);
        }

        [HttpPost("deleteclaim")]
        [ApiDocumentation(ClaimsDocumentationKeys.DeleteClaim)]
        public async Task<IActionResult> DeleteClaim([FromBody] UserClaimsRequest request)
        {
            Result<string> result = await _claimsService.DeleteClaims(request);

            return result.ToActionResult(this);
        }

        [HttpPost("editclaim")]
        [ApiDocumentation(ClaimsDocumentationKeys.EditClaim)]
        public async Task<IActionResult> EditClaim([FromBody] UserClaimsRequest request)
        {
            Result<UserClaimsResponse> result = await _claimsService.EditUserClaims(request);

            return result.ToActionResult(this);
        }

        [HttpGet("userclaims")]
        [ApiDocumentation(ClaimsDocumentationKeys.GetUserClaims)]
        public async Task<IActionResult> GetUserClaims([FromQuery] string email)
        {
            Result<IEnumerable<UserClaimsResponse>> result = await _claimsService.GetUserClaims(email);

            return result.ToActionResult(this);
        }
    }
}