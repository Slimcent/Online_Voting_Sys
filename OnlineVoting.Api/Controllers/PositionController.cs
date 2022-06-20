using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet("all-paged-positions", Name = "Get-All-Paged-Positions")]
        public async Task<IActionResult> GetAll([FromQuery] PositionRequestDto request)
        {
            PagedResponse<PositionResponseDto> all = await _positionService.AllPositions(request);

            return Ok(all);
        }

        [HttpGet("all-paged-active-positions", Name = "Get-All-Paged-Active-Positions")]
        public async Task<IActionResult> AllPagedActivePositions([FromQuery] PositionRequestDto request)
        {
            PagedResponse<PositionResponseDto> all = await _positionService.AllActivePositions(request);

            return Ok(all);
        }

        [HttpGet("all-paged-deleted-positions", Name = "Get-All-Paged-Deleted-Positions")]
        public async Task<IActionResult> AllPagedDeletedPositions([FromQuery] PositionRequestDto request)
        {
            PagedResponse<PositionResponseDto> all = await _positionService.AllDeletedPositions(request);

            return Ok(all);
        }

        [HttpGet("all-positions", Name = "All-Positions")]
        public async Task<IActionResult> GetAllPosition()
        {
            IEnumerable<PositionResponseDto> allPositions = await _positionService.GetAllPositions();

            return Ok(allPositions);
        }

        [HttpGet("all-active-positions", Name = "All-Active-Positions")]
        public async Task<IActionResult> GetAllActivePosition()
        {
            IEnumerable<PositionResponseDto> allActivePositions = await _positionService.GetAllActivePositions();

            return Ok(allActivePositions);
        }

        [HttpGet("all-deleted-positions", Name = "All-Deleted-Positions")]
        public async Task<IActionResult> GetAllDeletedPosition()
        {
            IEnumerable<PositionResponseDto> allDeletedPositions = await _positionService.GetAllDeletedPositions();

            return Ok(allDeletedPositions);
        }

        [HttpGet("position-by-id", Name = "Position-By-Name")]
        public async Task<IActionResult> GetStaffById(Guid id)
        {
            PositionResponseDto position = await _positionService.GetAPosition(id);

            return Ok(position);
        }

        [HttpPost("create-position", Name = "Create-Position")]
        public async Task<IActionResult> CreatePosition([FromQuery] PositionDto model)
        {
            string position = await _positionService.CreatePosition(model);

            return Ok(position);
        }

        [HttpPatch("patch-position", Name = "Patch-Position")]
        public async Task<IActionResult> PatchPosition(Guid Id, JsonPatchDocument<PositionDto> model)
        {
            string position = await _positionService.PatchPosition(Id, model);

            return Ok(position);
        }

        [HttpPut("update-position", Name = "Update-Position")]
        public async Task<IActionResult> UpdatePosition(Guid Id, PositionDto model)
        {
            string position = await _positionService.UpdatePosition(Id, model);

            return Ok(position);
        }

        [HttpDelete("delete-position-by-id", Name = "Delete-Position-By-Id")]
        public async Task<IActionResult> DeleteStaff([FromQuery] Guid id)
        {
            string toggle = await _positionService.DeletePosition(id);

            return Ok(toggle);
        }
    }
}
