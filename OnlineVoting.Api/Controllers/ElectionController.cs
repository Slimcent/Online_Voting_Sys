using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Extensions;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Api.Documentation.Definitions.Keys;

namespace OnlineVoting.Api.Controllers
{
    public class ElectionController : BaseController
    {
        private readonly IElectionService _electionService;

        public ElectionController(IElectionService electionService)
        {
            _electionService = electionService;
        }

        [HttpPost("create-election", Name = "Create-Election")]
        [ApiDocumentation(ElectionDocumentationKeys.CreateElection)]
        public async Task<IActionResult> CreateElection([FromBody] CreateElectionRequest request)
        {
            Result<string> result = await _electionService.CreateElection(request);
            return result.ToActionResult(this);
        }

        [HttpGet("elections", Name = "Get-Elections")]
        [ApiDocumentation(ElectionDocumentationKeys.GetElections)]
        public async Task<IActionResult> GetElections([FromQuery] ElectionRequest request)
        {
            Result<IEnumerable<ElectionResponse>> result = await _electionService.GetElections(request);
            return result.ToActionResult(this);
        }

        [HttpGet("paged-elections", Name = "Get-Paged-Elections")]
        [ApiDocumentation(ElectionDocumentationKeys.GetPagedElections)]
        public async Task<IActionResult> GetPagedElections([FromQuery] ElectionRequest request)
        {
            Result<PagedResponse<ElectionResponse>> result = await _electionService.GetPagedElections(request);
            return result.ToActionResult(this);
        }

        [HttpGet("election/{id}", Name = "Get-Election")]
        [ApiDocumentation(ElectionDocumentationKeys.GetElection)]
        public async Task<IActionResult> GetElection(string id)
        {
            Result<ElectionResponse> result = await _electionService.GetElection(id);
            return result.ToActionResult(this);
        }

        [HttpPut("update-election", Name = "Update-Election")]
        [ApiDocumentation(ElectionDocumentationKeys.UpdateElection)]
        public async Task<IActionResult> UpdateElection([FromBody] CreateElectionRequest request)
        {
            Result<string> result = await _electionService.UpdateElection(request);
            return result.ToActionResult(this);
        }

        [HttpPatch("election-activation/{id}", Name = "Toggle-Election-Activation")]
        [ApiDocumentation(ElectionDocumentationKeys.ToggleElectionActivation)]
        public async Task<IActionResult> ToggleElectionActivation(string id)
        {
            Result<string> result = await _electionService.ToggleElectionActivation(id);
            return result.ToActionResult(this);
        }

        [HttpGet("election-statuses", Name = "Get-Election-Statuses")]
        [ApiDocumentation(ElectionDocumentationKeys.GetElectionStatuses)]
        public async Task<IActionResult> GetElectionStatuses()
        {
            Result<IEnumerable<ElectionStatusResponse>> result = await _electionService.GetElectionStatuses();
            return result.ToActionResult(this);
        }

        [HttpGet("paged-election-statuses", Name = "Get-Paged-Election-Statuses")]
        [ApiDocumentation(ElectionDocumentationKeys.GetPagedElectionStatuses)]
        public async Task<IActionResult> GetPagedElectionStatuses([FromQuery] ElectionStatusRequest request)
        {
            Result<PagedResponse<ElectionStatusResponse>> result = await _electionService.GetPagedElectionStatuses(request);
            return result.ToActionResult(this);
        }

        [HttpGet("election-status/{id:int}", Name = "Get-Election-Status")]
        [ApiDocumentation(ElectionDocumentationKeys.GetElectionStatus)]
        public async Task<IActionResult> GetElectionStatus(int id)
        {
            Result<ElectionStatusResponse> result = await _electionService.GetElectionStatus(id);
            return result.ToActionResult(this);
        }

        [HttpPut("update-election-status", Name = "Update-Election-Status")]
        [ApiDocumentation(ElectionDocumentationKeys.UpdateElectionStatus)]
        public async Task<IActionResult> UpdateElectionStatus([FromBody] UpdateElectionStatusRequest request)
        {
            Result<string> result = await _electionService.UpdateElectionStatus(request);
            return result.ToActionResult(this);
        }

        [HttpGet("election-scopes", Name = "Get-Election-Scopes")]
        [ApiDocumentation(ElectionDocumentationKeys.GetElectionScopes)]
        public async Task<IActionResult> GetElectionScopes()
        {
            Result<IEnumerable<ElectionScopeResponse>> result = await _electionService.GetElectionScopes();
            return result.ToActionResult(this);
        }

        [HttpGet("paged-election-scopes", Name = "Get-Paged-Election-Scopes")]
        [ApiDocumentation(ElectionDocumentationKeys.GetPagedElectionScopes)]
        public async Task<IActionResult> GetPagedElectionScopes([FromQuery] ElectionScopeRequest request)
        {
            Result<PagedResponse<ElectionScopeResponse>> result = await _electionService.GetPagedElectionScopes(request);
            return result.ToActionResult(this);
        }

        [HttpGet("election-scope/{id:int}", Name = "Get-Election-Scope")]
        [ApiDocumentation(ElectionDocumentationKeys.GetElectionScope)]
        public async Task<IActionResult> GetElectionScope(int id)
        {
            Result<ElectionScopeResponse> result = await _electionService.GetElectionScope(id);
            return result.ToActionResult(this);
        }

        [HttpPut("update-election-scope", Name = "Update-Election-Scope")]
        [ApiDocumentation(ElectionDocumentationKeys.UpdateElectionScope)]
        public async Task<IActionResult> UpdateElectionScope([FromBody] UpdateElectionScopeRequest request)
        {
            Result<string> result = await _electionService.UpdateElectionScope(request);
            return result.ToActionResult(this);
        }

        [HttpPatch("election-scope-activation/{id:int}", Name = "Election-Scope-Activation")]
        [ApiDocumentation(ElectionDocumentationKeys.ToggleElectionScopeActivation)]
        public async Task<IActionResult> ToggleElectionScopeActivation(int id)
        {
            Result<string> result = await _electionService.ToggleElectionScopeActivation(id);
            return result.ToActionResult(this);
        }
    }
}