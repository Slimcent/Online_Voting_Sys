using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Caching.Keys
{
    public static class RegisteredVoterCacheKeys
    {
        public static string GetRegisteredVoter(string registeredVoterId)
        {
            return $"registered-voter:{registeredVoterId}";
        }

        public static string GetRegisteredVoters(RegisteredVoterRequest request)
        {
            return $"registered-voters:" +
                $"page:{request.PageNumber}:" +
                $"size:{request.PageSize}:" +
                $"election:{request.ElectionId ?? "all"}:" +
                $"student:{request.StudentId?.ToString() ?? "all"}:" +
                $"active:{request.Active?.ToString() ?? "all"}:" +
                $"search:{request.SearchTerm?.Trim().ToLowerInvariant() ?? "none"}";
        }
    }
}