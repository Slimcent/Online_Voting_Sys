
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IElectionTypeService
    {
        Task<Result<string>> CreateElectionType(CreateElectionTypeRequest request);
        Task<Result<IEnumerable<ElectionTypeResponse>>> GetElectionTypes();
        Task<Result<ElectionTypeResponse>> GetElectionType(int id);
        Task<Result<PagedResponse<ElectionTypeResponse>>> GetPagedElectionTypes(ElectionTypeRequest request);
        Task<Result<string>> UpdateElectionType(CreateElectionTypeRequest request);
        Task<Result<string>> ToggleElectionTypeActivation(int id);
        Task<Result<string>> DeleteElectionType(int id);
    }
}
