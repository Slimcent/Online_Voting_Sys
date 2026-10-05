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
    public class ElectionPositionController : BaseController
    {
        private readonly IElectionPositionService _electionPositionService;

        public ElectionPositionController(IElectionPositionService electionPositionService)
        {
            _electionPositionService = electionPositionService;
        }

        [HttpPost("create-election-position", Name = "Create-Election-Position")]
        [ApiDocumentation(ElectionPositionDocumentationKeys.CreateElectionPosition)]
        public async Task<IActionResult> CreateElectionPosition([FromBody] CreateElectionPositionRequest request)
        {
            Result<string> result = await _electionPositionService.CreateElectionPosition(request);
            return result.ToActionResult(this);
        }

        [HttpPost("create-election-positions", Name = "Create-Election-Positions")]
        [ApiDocumentation(ElectionPositionDocumentationKeys.CreateElectionPositions)]
        public async Task<IActionResult> CreateElectionPositions([FromBody] CreateElectionPositionsRequest request)
        {
            Result<string> result = await _electionPositionService.CreateElectionPositions(request);
            return result.ToActionResult(this);
        }

        [HttpPut("update-election-position", Name = "Update-Election-Position")]
        [ApiDocumentation(ElectionPositionDocumentationKeys.UpdateElectionPosition)]
        public async Task<IActionResult> UpdateElectionPosition([FromBody] CreateElectionPositionRequest request)
        {
            Result<string> result = await _electionPositionService.UpdateElectionPosition(request);
            return result.ToActionResult(this);
        }

        [HttpPatch("election-position-activation/{id}", Name = "Toggle-Election-Position-Activation")]
        [ApiDocumentation(ElectionPositionDocumentationKeys.ToggleElectionPositionActivation)]
        public async Task<IActionResult> ToggleElectionPositionActivation(string id)
        {
            Result<string> result = await _electionPositionService.ToggleElectionPositionActivation(id);
            return result.ToActionResult(this);
        }

        [HttpDelete("delete-election-position/{id}", Name = "Delete-Election-Position")]
        [ApiDocumentation(ElectionPositionDocumentationKeys.DeleteElectionPosition)]
        public async Task<IActionResult> DeleteElectionPosition(string id)
        {
            Result<string> result = await _electionPositionService.DeleteElectionPosition(id);
            return result.ToActionResult(this);
        }

        [HttpGet("election-positions", Name = "Get-Election-Positions")]
        [ApiDocumentation(ElectionPositionDocumentationKeys.GetElectionPositions)]
        public async Task<IActionResult> GetElectionPositions([FromQuery] ElectionPositionRequest request)
        {
            Result<PagedResponse<ElectionPositionResponse>> result = await _electionPositionService.GetElectionPositions(request);
            return result.ToActionResult(this);
        }

        [HttpGet("election-positions-with-applications", Name = "Get-Election-Positions-With-Applications")]
        [ApiDocumentation(ElectionPositionDocumentationKeys.GetElectionPositionsWithApplications)]
        public async Task<IActionResult> GetElectionPositionsWithApplications([FromQuery] ElectionPositionRequest request)
        {
            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await _electionPositionService.GetElectionPositionsWithApplications(request);
            return result.ToActionResult(this);
        }
    }
}