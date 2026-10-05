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
    public class ElectionTypeController : ControllerBase
    {
        private readonly IElectionTypeService _electionTypeService;

        public ElectionTypeController(IElectionTypeService electionTypeService)
        {
            _electionTypeService = electionTypeService;
        }

        [HttpPost("create-election-type", Name = "Create-Election-Type")]
        [ApiDocumentation(ElectionTypeDocumentationKeys.CreateElectionType)]
        public async Task<IActionResult> CreateElectionType([FromBody] CreateElectionTypeRequest request)
        {
            Result<string> result = await _electionTypeService.CreateElectionType(request);
            return result.ToActionResult(this);
        }

        [HttpGet("election-types", Name = "Get-Election-Types")]
        [ApiDocumentation(ElectionTypeDocumentationKeys.GetElectionTypes)]
        public async Task<IActionResult> GetElectionTypes()
        {
            Result<IEnumerable<ElectionTypeResponse>> result = await _electionTypeService.GetElectionTypes();
            return result.ToActionResult(this);
        }

        [HttpGet("election-type/{id:int}", Name = "Get-Election-Type")]
        [ApiDocumentation(ElectionTypeDocumentationKeys.GetElectionType)]
        public async Task<IActionResult> GetElectionType(int id)
        {
            Result<ElectionTypeResponse> result = await _electionTypeService.GetElectionType(id);
            return result.ToActionResult(this);
        }

        [HttpPut("update-election-type", Name = "Update-Election-Type")]
        [ApiDocumentation(ElectionTypeDocumentationKeys.UpdateElectionType)]
        public async Task<IActionResult> UpdateElectionType([FromBody] CreateElectionTypeRequest request)
        {
            Result<string> result = await _electionTypeService.UpdateElectionType(request);
            return result.ToActionResult(this);
        }

        [HttpPatch("election-type-activation/{id:int}", Name = "Election-Type-Activation")]
        [ApiDocumentation(ElectionTypeDocumentationKeys.ToggleElectionTypeActivation)]
        public async Task<IActionResult> ToggleElectionTypeActivation(int id)
        {
            Result<string> result = await _electionTypeService.ToggleElectionTypeActivation(id);
            return result.ToActionResult(this);
        }

        [HttpDelete("delete-election-type/{id:int}", Name = "Delete-Election-Type")]
        [ApiDocumentation(ElectionTypeDocumentationKeys.DeleteElectionType)]
        public async Task<IActionResult> DeleteElectionType(int id)
        {
            Result<string> result = await _electionTypeService.DeleteElectionType(id);
            return result.ToActionResult(this);
        }

        [HttpGet("paged-election-types", Name = "Get-Paged-Election-Types")]
        [ApiDocumentation(ElectionTypeDocumentationKeys.GetPagedElectionTypes)]
        public async Task<IActionResult> GetPagedElectionTypes([FromQuery] ElectionTypeRequest request)
        {
            Result<PagedResponse<ElectionTypeResponse>> result = await _electionTypeService.GetPagedElectionTypes(request);
            return result.ToActionResult(this);
        }
    }
}