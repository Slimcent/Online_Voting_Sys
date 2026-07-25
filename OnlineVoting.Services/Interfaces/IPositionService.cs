using Microsoft.AspNetCore.JsonPatch;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Services.Interfaces
{
    public interface IPositionService
    {
        Task<string> CreatePosition(CreateWithNameRequest request);
        Task<string> UpdatePosition(Guid positionId, CreateWithNameRequest request);
        Task<string> DeletePosition(Guid id);
        Task<PositionResponse> GetAPosition(Guid positionId);
        Task<string> PatchPosition(Guid positionId, JsonPatchDocument<CreateWithNameRequest> request);
        Task<IEnumerable<PositionResponse>> GetAllPositions();
        Task<IEnumerable<PositionResponse>> GetAllDeletedPositions();
        Task<IEnumerable<PositionResponse>> GetAllActivePositions();
        Task<PagedResponse<PositionResponse>> AllPositions(PositionRequest request);
        Task<PagedResponse<PositionResponse>> AllActivePositions(PositionRequest request);
        Task<PagedResponse<PositionResponse>> AllDeletedPositions(PositionRequest request);
    }
}