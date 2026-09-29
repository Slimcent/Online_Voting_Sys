using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IElectionPositionService
    {
        Task<Result<string>> CreateElectionPosition(CreateElectionPositionRequest request);
        Task<Result<string>> CreateElectionPositions(CreateElectionPositionsRequest request);
        Task<Result<string>> UpdateElectionPosition(CreateElectionPositionRequest request);
        Task<Result<string>> ToggleElectionPositionActivation(string id);
        Task<Result<string>> DeleteElectionPosition(string id);
        Task<Result<PagedResponse<ElectionPositionResponse>>> GetElectionPositions(ElectionPositionRequest request);
        Task<Result<PagedResponse<ElectionPositionWithApplicationsResponse>>> GetElectionPositionsWithApplications(ElectionPositionRequest request);
    }
}
