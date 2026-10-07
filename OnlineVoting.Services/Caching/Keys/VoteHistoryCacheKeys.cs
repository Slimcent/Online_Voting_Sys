using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Caching.Keys
{
    public static class VoteHistoryCacheKeys
    {
        public static string GetMyVotes(string userId, VoteHistoryRequest request)
        {
            return $"vote-history:{userId}:{request.ElectionId}:{request.ElectionPositionId}:{request.SearchTerm}:{request.PageNumber}:{request.PageSize}:{request.OrderBy}";
        }
    }
}
