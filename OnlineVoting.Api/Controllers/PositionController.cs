using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Api.Documentation.Attributes;
using OnlineVoting.Api.Documentation.Definitions.Keys;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
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
            PagedResponse<PositionResponse> all = await _positionService.AllPositions(request);

            return Ok(all);
        }

        [HttpGet("all-paged-active-positions", Name = "Get-All-Paged-Active-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllPagedActivePositions)]
        public async Task<IActionResult> AllPagedActivePositions([FromQuery] PositionRequest request)
        {
            PagedResponse<PositionResponse> all = await _positionService.AllActivePositions(request);

            return Ok(all);
        }

        [HttpGet("all-paged-deleted-positions", Name = "Get-All-Paged-Deleted-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllPagedDeletedPositions)]
        public async Task<IActionResult> AllPagedDeletedPositions([FromQuery] PositionRequest request)
        {
            PagedResponse<PositionResponse> all = await _positionService.AllDeletedPositions(request);

            return Ok(all);
        }

        [HttpGet("all-positions", Name = "All-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllPositions)]
        public async Task<IActionResult> GetAllPosition()
        {
            IEnumerable<PositionResponse> allPositions = await _positionService.GetAllPositions();

            return Ok(allPositions);
        }

        [HttpGet("all-active-positions", Name = "All-Active-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllActivePositions)]
        public async Task<IActionResult> GetAllActivePosition()
        {
            IEnumerable<PositionResponse> allActivePositions = await _positionService.GetAllActivePositions();

            return Ok(allActivePositions);
        }

        [HttpGet("all-deleted-positions", Name = "All-Deleted-Positions")]
        [ApiDocumentation(PositionDocumentationKeys.GetAllDeletedPositions)]
        public async Task<IActionResult> GetAllDeletedPosition()
        {
            IEnumerable<PositionResponse> allDeletedPositions = await _positionService.GetAllDeletedPositions();

            return Ok(allDeletedPositions);
        }

        [HttpGet("position-by-id", Name = "Position-By-Id")]
        [ApiDocumentation(PositionDocumentationKeys.GetPositionById)]
        public async Task<IActionResult> GetPositionById([FromQuery] Guid id)
        {
            PositionResponse position = await _positionService.GetAPosition(id);

            return Ok(position);
        }

        [HttpPost("create-position", Name = "Create-Position")]
        [ApiDocumentation(PositionDocumentationKeys.CreatePosition)]
        public async Task<IActionResult> CreatePosition([FromBody] CreateWithNameRequest request)
        {
            string position = await _positionService.CreatePosition(request);

            return Ok(position);
        }

        [HttpPatch("patch-position", Name = "Patch-Position")]
        [ApiDocumentation(PositionDocumentationKeys.PatchPosition)]
        public async Task<IActionResult> PatchPosition([FromQuery] Guid id, [FromBody] JsonPatchDocument<CreateWithNameRequest> request)
        {
            string position = await _positionService.PatchPosition(id, request);

            return Ok(position);
        }

        [HttpPut("update-position", Name = "Update-Position")]
        [ApiDocumentation(PositionDocumentationKeys.UpdatePosition)]
        public async Task<IActionResult> UpdatePosition([FromQuery] Guid id, [FromBody] CreateWithNameRequest request)
        {
            string position = await _positionService.UpdatePosition(id, request);

            return Ok(position);
        }

        [HttpDelete("delete-position-by-id", Name = "Delete-Position-By-Id")]
        [ApiDocumentation(PositionDocumentationKeys.DeletePosition)]
        public async Task<IActionResult> DeletePosition([FromQuery] Guid id)
        {
            string toggle = await _positionService.DeletePosition(id);

            return Ok(toggle);
        }
    }
}