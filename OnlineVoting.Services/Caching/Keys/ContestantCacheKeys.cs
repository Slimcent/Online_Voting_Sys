using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Caching.Keys
{
    public static class ContestantCacheKeys
    {
        private const string Prefix = "onlinevoting:v1:contestants";

        public static string GetContestants(ContestantRequest request)
        {
            return $"{Prefix}:page:{request.PageNumber}:size:{request.PageSize}:election:{request.ElectionId}:position:{request.PositionId}:active:{request.Active}:search:{request.SearchTerm}";
        }

        public static string GetContestant(string contestantId)
        {
            return $"{Prefix}:id:{contestantId}";
        }
    }
}
