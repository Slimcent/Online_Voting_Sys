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
        Task<Result<string>> CastVote(CastVoteRequest request);
        Task<Result<PagedResponse<VoteHistoryResponse>>> GetMyVotes(VoteHistoryRequest request);
        Task<Result<PagedResponse<ElectionResultResponse>>> GetElectionResults(ElectionResultRequest request);
    }
}