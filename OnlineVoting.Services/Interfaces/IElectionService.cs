using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IElectionService
    {
        // Election status methods
        Task<Result<IEnumerable<ElectionStatusResponse>>> GetElectionStatuses();
        Task<Result<PagedResponse<ElectionStatusResponse>>> GetPagedElectionStatuses(ElectionStatusRequest request);
        Task<Result<ElectionStatusResponse>> GetElectionStatus(int id);
        Task<Result<string>> UpdateElectionStatus(UpdateElectionStatusRequest request);

        // Election scope methods
        Task<Result<IEnumerable<ElectionScopeResponse>>> GetElectionScopes();
        Task<Result<PagedResponse<ElectionScopeResponse>>> GetPagedElectionScopes(ElectionScopeRequest request);
        Task<Result<ElectionScopeResponse>> GetElectionScope(int id);
        Task<Result<string>> UpdateElectionScope(UpdateElectionScopeRequest request);
        Task<Result<string>> ToggleElectionScopeActivation(int id);

        // Election methods
        Task<Result<string>> CreateElection(CreateElectionRequest request);
        Task<Result<IEnumerable<ElectionResponse>>> GetElections(ElectionRequest request);
        Task<Result<PagedResponse<ElectionResponse>>> GetPagedElections(ElectionRequest request);
        Task<Result<ElectionResponse>> GetElection(string id);
        Task<Result<string>> UpdateElection(CreateElectionRequest request);
        Task<Result<string>> ToggleElectionActivation(string id);
    }
}
