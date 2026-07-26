using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
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
    public class PositionController : BaseController
    {
        private readonly IPositionService _positionService;

        public PositionController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet("all-paged-positions", Name = "Get-All-Paged-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllPagedPositions)]
        public async Task<IActionResult> GetAll([FromQuery] PositionRequest request)
        {
            Result<PagedResponse<PositionResponse>> result = await _positionService.AllPositions(request);

            return result.ToActionResult(this);
        }

        [HttpGet("all-paged-active-positions", Name = "Get-All-Paged-Active-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllPagedActivePositions)]
        public async Task<IActionResult> AllPagedActivePositions([FromQuery] PositionRequest request)
        {
            Result<PagedResponse<PositionResponse>> result = await _positionService.AllActivePositions(request);

            return result.ToActionResult(this);
        }

        [HttpGet("all-paged-deleted-positions", Name = "Get-All-Paged-Deleted-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllPagedDeletedPositions)]
        public async Task<IActionResult> AllPagedDeletedPositions([FromQuery] PositionRequest request)
        {
            Result<PagedResponse<PositionResponse>> result = await _positionService.AllDeletedPositions(request);

            return result.ToActionResult(this);
        }

        [HttpGet("all-positions", Name = "All-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllPositions)]
        public async Task<IActionResult> GetAllPosition()
        {
            Result<IEnumerable<PositionResponse>> result = await _positionService.GetAllPositions();

            return result.ToActionResult(this);
        }

        [HttpGet("all-active-positions", Name = "All-Active-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllActivePositions)]
        public async Task<IActionResult> GetAllActivePosition()
        {
            Result<IEnumerable<PositionResponse>> result = await _positionService.GetAllActivePositions();

            return result.ToActionResult(this);
        }

        [HttpGet("all-deleted-positions", Name = "All-Deleted-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllDeletedPositions)]
        public async Task<IActionResult> GetAllDeletedPosition()
        {
            Result<IEnumerable<PositionResponse>> result = await _positionService.GetAllDeletedPositions();

            return result.ToActionResult(this);
        }

        [HttpGet("position-by-id", Name = "Position-By-Id")]
        [ApiDocumentation(PositionDocumentationKeys.GetPositionById)]
        public async Task<IActionResult> GetPositionById([FromQuery] Guid id)
        {
            Result<PositionResponse> result = await _positionService.GetAPosition(id);

            return result.ToActionResult(this);
        }

        [HttpPost("create-position", Name = "Create-Position")]
        [ApiDocumentation(PositionDocumentationKeys.CreatePosition)]
        public async Task<IActionResult> CreatePosition([FromBody] CreateWithNameRequest request)
        {
            Result<string> result = await _positionService.CreatePosition(request);

            return result.ToActionResult(this);
        }

        [HttpPatch("patch-position", Name = "Patch-Position")]
        [ApiDocumentation(PositionDocumentationKeys.PatchPosition)]
        public async Task<IActionResult> PatchPosition([FromQuery] Guid id, [FromBody] JsonPatchDocument<CreateWithNameRequest> request)
        {
            Result<string> result = await _positionService.PatchPosition(id, request);

            return result.ToActionResult(this);
        }

        [HttpPut("update-position", Name = "Update-Position")]
        [ApiDocumentation(PositionDocumentationKeys.UpdatePosition)]
        public async Task<IActionResult> UpdatePosition([FromQuery] Guid id, [FromBody] CreateWithNameRequest request)
        {
            Result<string> result = await _positionService.UpdatePosition(id, request);

            return result.ToActionResult(this);
        }

        [HttpDelete("delete-position-by-id", Name = "Delete-Position-By-Id")]
        [ApiDocumentation(PositionDocumentationKeys.DeletePosition)]
        public async Task<IActionResult> DeletePosition([FromQuery] Guid id)
        {
            Result<string> result = await _positionService.DeletePosition(id);

            return result.ToActionResult(this);
        }
    }
}