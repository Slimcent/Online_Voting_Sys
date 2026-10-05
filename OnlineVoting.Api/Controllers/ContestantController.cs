using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Models.Results;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Api.Extensions;

namespace OnlineVoting.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ContestantController : BaseController
    {
        private readonly IContestantService _contestantService;

        public ContestantController(IContestantService contestantService)
        {
            _contestantService = contestantService;
        }

        [HttpGet("contestants", Name = "Get-Contestants")]
        [ApiDocumentation(ContestantDocumentationKeys.GetContestants)]
        public async Task<IActionResult> GetContestants([FromQuery] ContestantRequest request)
        {
            Result<PagedResponse<ContestantResponse>> result = await _contestantService.GetContestants(request);

            return result.ToActionResult(this);
        }

        [HttpGet("contestant/{contestantId}", Name = "Get-Contestant")]
        [ApiDocumentation(ContestantDocumentationKeys.GetContestant)]
        public async Task<IActionResult> GetContestant(string contestantId)
        {
            Result<ContestantResponse> result = await _contestantService.GetContestant(contestantId);

            return result.ToActionResult(this);
        }

        [HttpPatch("contestant-activation/{contestantId}", Name = "Toggle-Contestant-Activation")]
        [ApiDocumentation(ContestantDocumentationKeys.ToggleContestantActivation)]
        public async Task<IActionResult> ToggleContestantActivation(string contestantId)
        {
            Result<string> result = await _contestantService.ToggleContestantActivation(contestantId);

            return result.ToActionResult(this);
        }
    }
}