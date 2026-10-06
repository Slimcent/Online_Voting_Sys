using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Caching.Keys
{
    public static class ElectionResultCacheKeys
    {
        public static string GetElectionResults(ElectionResultRequest request)
        {
            return $"election-results:{request.ElectionId}:{request.ElectionPositionId}:{request.SearchTerm}:{request.PageNumber}:{request.PageSize}:{request.OrderBy}";
        }
    }
}