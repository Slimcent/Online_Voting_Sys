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
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class PositionApplicationController : BaseController
    {
        private readonly IPositionApplicationService _positionApplicationService;

        public PositionApplicationController(IPositionApplicationService positionApplicationService)
        {
            _positionApplicationService = positionApplicationService;
        }

        [HttpPost("create-position-application", Name = "Create-Position-Application")]
        [ApiDocumentation(PositionApplicationDocumentationKeys.CreatePositionApplication)]
        public async Task<IActionResult> CreatePositionApplication(CreatePositionApplicationRequest request)
        {
            Result<CreatePositionApplicationResponse> result = await _positionApplicationService.CreatePositionApplication(request);

            return result.ToActionResult(this);
        }

        [HttpPatch("cancel-position-application/{positionApplicationId}", Name = "Cancel-Position-Application")]
        [ApiDocumentation(PositionApplicationDocumentationKeys.CancelPositionApplication)]
        public async Task<IActionResult> CancelPositionApplication(string positionApplicationId)
        {
            Result<string> result = await _positionApplicationService.CancelPositionApplication(positionApplicationId);

            return result.ToActionResult(this);
        }

        [HttpGet("my-position-applications", Name = "Get-My-Position-Applications")]
        [ApiDocumentation(PositionApplicationDocumentationKeys.GetMyPositionApplications)]
        public async Task<IActionResult> GetMyPositionApplications([FromQuery] PositionApplicationRequest request)
        {
            Result<PagedResponse<PositionApplicationResponse>> result = await _positionApplicationService.GetMyPositionApplications(request);

            return result.ToActionResult(this);
        }

        [HttpGet("my-position-application/{positionApplicationId}", Name = "Get-My-Position-Application")]
        [ApiDocumentation(PositionApplicationDocumentationKeys.GetMyPositionApplication)]
        public async Task<IActionResult> GetMyPositionApplication(string positionApplicationId)
        {
            Result<PositionApplicationResponse> result = await _positionApplicationService.GetPositionApplication(positionApplicationId);

            return result.ToActionResult(this);
        }

        [HttpGet("position-applications", Name = "Get-Position-Applications")]
        [ApiDocumentation(PositionApplicationDocumentationKeys.GetPositionApplications)]
        public async Task<IActionResult> GetPositionApplications([FromQuery] PositionApplicationRequest request)
        {
            Result<PagedResponse<PositionApplicationResponse>> result = await _positionApplicationService.GetPositionApplications(request);

            return result.ToActionResult(this);
        }

        [HttpGet("position-application/{positionApplicationId}", Name = "Get-Position-Application")]
        [ApiDocumentation(PositionApplicationDocumentationKeys.GetPositionApplication)]
        public async Task<IActionResult> GetPositionApplication(string positionApplicationId)
        {
            Result<PositionApplicationResponse> result = await _positionApplicationService.GetPositionApplication(positionApplicationId);

            return result.ToActionResult(this);
        }

        [HttpPatch("approve-or-reject-position-application", Name = "Approve-Or-Reject-Position-Application")]
        [ApiDocumentation(PositionApplicationDocumentationKeys.ApproveOrRejectPositionApplication)]
        public async Task<IActionResult> ApproveOrRejectPositionApplication(ApproveOrRejectPositionApplicationRequest request)
        {
            Result<string> result = await _positionApplicationService.ApproveOrRejectPositionApplication(request);

            return result.ToActionResult(this);
        }

        [HttpGet("position-applications-with-contestants", Name = "Get-Position-Applications-With-Contestants")]
        [ApiDocumentation(PositionApplicationDocumentationKeys.GetPositionApplicationsWithContestants)]
        public async Task<IActionResult> GetPositionApplicationsWithContestants([FromQuery] PositionApplicationWithContestantRequest request)
        {
            Result<PagedResponse<PositionApplicationResponse>> result = await _positionApplicationService.GetPositionApplicationsWithContestants(request);

            return result.ToActionResult(this);
        }
    }
}