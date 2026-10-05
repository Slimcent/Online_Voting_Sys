using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Caching.Keys
{
    public static class PositionApplicationCacheKeys
    {
        private const string Prefix = "onlinevoting:v1:position-application";

        public static string GetMyPositionApplications(string userId, PositionApplicationRequest request)
        {
            string searchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? "all" : request.SearchTerm.Trim().ToLowerInvariant();
            string positionApplicationStatusId = request.PositionApplicationStatusId?.ToString() ?? "all";

            return $"{Prefix}:user:{userId}:list:page:{request.PageNumber}:size:{request.PageSize}:status:{positionApplicationStatusId}:search:{searchTerm}";
        }

        public static string GetPositionApplication(string userId, string positionApplicationId)
        {
            return $"{Prefix}:user:{userId}:id:{positionApplicationId}";
        }
    }
}