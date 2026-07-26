using Microsoft.AspNetCore.JsonPatch;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IPositionService
    {
        Task<Result<string>> CreatePosition(CreateWithNameRequest request);
        Task<Result<string>> UpdatePosition(Guid positionId, CreateWithNameRequest request);
        Task<Result<string>> DeletePosition(Guid id);
        Task<Result<PositionResponse>> GetAPosition(Guid positionId);
        Task<Result<string>> PatchPosition(Guid positionId, JsonPatchDocument<CreateWithNameRequest> request);
        Task<Result<IEnumerable<PositionResponse>>> GetAllPositions();
        Task<Result<IEnumerable<PositionResponse>>> GetAllDeletedPositions();
        Task<Result<IEnumerable<PositionResponse>>> GetAllActivePositions();
        Task<Result<PagedResponse<PositionResponse>>> AllPositions(PositionRequest request);
        Task<Result<PagedResponse<PositionResponse>>> AllActivePositions(PositionRequest request);
        Task<Result<PagedResponse<PositionResponse>>> AllDeletedPositions(PositionRequest request);
    }
}