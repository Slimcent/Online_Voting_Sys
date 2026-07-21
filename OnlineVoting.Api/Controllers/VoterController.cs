
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Policy = "Authorization")]
    public class VoterController : ControllerBase
    {
        private readonly IVoterService _voterService;

        public VoterController(IVoterService voterService)
        {
            _voterService = voterService;
        }

        [HttpPost("create-voter", Name = "Create-Voter")]
        public async Task<IActionResult> CreateVoter([FromQuery] CreateVoterRequest model)
        {
            string voter = await _voterService.CreateVoter(model);

            return Ok(voter);
        }

        [HttpPut("toggle-voter-status", Name = "Toggle-Voter-Status")]
        public async Task<IActionResult> ToggleVoterStatus([FromQuery] Guid id)
        {
            string voter = await _voterService.ToggleVoter(id);

            return Ok(voter);
        }
    }
}