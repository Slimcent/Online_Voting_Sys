using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Services.Interfaces
{
    public interface IVoterService
    {
        Task<Result<RegisteredVoterResponse>> RegisterVoter(RegisterVoterRequest request);
        Task<Result<RegisteredVoterResponse>> GetRegisteredVoter(string registeredVoterId);
        Task<Result<PagedResponse<RegisteredVoterResponse>>> GetRegisteredVoters(RegisteredVoterRequest request);
    }
}