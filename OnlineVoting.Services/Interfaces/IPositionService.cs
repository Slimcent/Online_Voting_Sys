using Microsoft.AspNetCore.JsonPatch;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Services.Interfaces
{
    public interface IPositionService
    {
        Task<string> CreatePosition(PositionDto request);
        Task<string> UpdatePosition(Guid positionId, PositionDto request);
        Task<string> DeletePosition(Guid id);
        Task<PositionResponseDto> GetAPosition(Guid positionId);
        Task<string> PatchPosition(Guid positionId, JsonPatchDocument<PositionDto> request);
        Task<IEnumerable<PositionResponseDto>> GetAllPositions();
        Task<IEnumerable<PositionResponseDto>> GetAllDeletedPositions();
        Task<IEnumerable<PositionResponseDto>> GetAllActivePositions();
        Task<PagedResponse<PositionResponseDto>> AllPositions(PositionRequestDto request);
        Task<PagedResponse<PositionResponseDto>> AllActivePositions(PositionRequestDto request);
        Task<PagedResponse<PositionResponseDto>> AllDeletedPositions(PositionRequestDto request);
    }
}
