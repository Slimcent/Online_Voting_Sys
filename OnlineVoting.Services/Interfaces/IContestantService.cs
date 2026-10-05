using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IContestantService
    {
        Task<Result<PagedResponse<ContestantResponse>>> GetContestants(ContestantRequest request);
        Task<Result<ContestantResponse>> GetContestant(string contestantId);
        Task<Result<string>> ToggleContestantActivation(string contestantId);
    }
}
