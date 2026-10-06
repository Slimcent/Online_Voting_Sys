using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Policy = "Authorization")]
    public class VoterController : BaseController
    {
        private readonly IVoterService _voterService;

        public VoterController(IVoterService voterService)
        {
            _voterService = voterService;
        }

        [HttpPost("register-voter", Name = "Register-Voter")]
        [ApiDocumentation(VoterDocumentationKeys.RegisterVoter)]
        public async Task<IActionResult> RegisterVoter([FromBody] RegisterVoterRequest request)
        {
            Result<RegisteredVoterResponse> result = await _voterService.RegisterVoter(request);

            return result.ToActionResult(this);
        }

        [HttpGet("{registeredVoterId}", Name = "Get-Registered-Voter")]
        [ApiDocumentation(VoterDocumentationKeys.GetRegisteredVoter)]
        public async Task<IActionResult> GetRegisteredVoter(string registeredVoterId)
        {
            Result<RegisteredVoterResponse> result = await _voterService.GetRegisteredVoter(registeredVoterId);

            return result.ToActionResult(this);
        }

        [HttpGet("registered-voters", Name = "Get-Registered-Voters")]
        [ApiDocumentation(VoterDocumentationKeys.GetRegisteredVoters)]
        public async Task<IActionResult> GetRegisteredVoters([FromQuery] RegisteredVoterRequest request)
        {
            Result<PagedResponse<RegisteredVoterResponse>> result = await _voterService.GetRegisteredVoters(request);

            return result.ToActionResult(this);
        }

        [HttpPost("cast-vote", Name = "Cast-Vote")]
        [ApiDocumentation(VoterDocumentationKeys.CastVote)]
        public async Task<IActionResult> CastVote([FromBody] CastVoteRequest request)
        {
            Result<string> result = await _voterService.CastVote(request);
            return result.ToActionResult(this);
        }

        [HttpGet("my-votes", Name = "Get-My-Votes")]
        [ApiDocumentation(VoterDocumentationKeys.GetMyVotes)]
        public async Task<IActionResult> GetMyVotes([FromQuery] VoteHistoryRequest request)
        {
            Result<PagedResponse<VoteHistoryResponse>> result = await _voterService.GetMyVotes(request);
            return result.ToActionResult(this);
        }

        [HttpGet("election-results", Name = "Get-Election-Results")]
        [ApiDocumentation(VoterDocumentationKeys.GetElectionResults)]
        public async Task<IActionResult> GetElectionResults([FromQuery] ElectionResultRequest request)
        {
            Result<PagedResponse<ElectionResultResponse>> result = await _voterService.GetElectionResults(request);
            return result.ToActionResult(this);
        }
    }
}