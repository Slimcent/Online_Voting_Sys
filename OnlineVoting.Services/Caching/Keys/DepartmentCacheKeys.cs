using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Caching.Keys
{
    public static class DepartmentCacheKeys
    {
        private const string Prefix = "onlinevoting:v1:department";

        public static string GetDepartment(long id)
        {
            return $"{Prefix}:id:{id}";
        }

        public static string GetDepartments(DepartmentRequestParameters request)
        {
            return $"{Prefix}:list:{BuildRequestKey(request)}";
        }

        public static string GetDepartmentsByFacultyId(long facultyId)
        {
            return $"{Prefix}:faculty:{facultyId}:all";
        }

        public static string GetDepartmentsByFacultyId(long facultyId, DepartmentRequestParameters request)
        {
            return $"{Prefix}:faculty:{facultyId}:list:{BuildRequestKey(request)}";
        }

        private static string BuildRequestKey(DepartmentRequestParameters request)
        {
            string searchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? "none" : Uri.EscapeDataString(request.SearchTerm.Trim());
            string orderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "none" : Uri.EscapeDataString(request.OrderBy.Trim());
            string facultyId = request.FacultyId.HasValue ? request.FacultyId.Value.ToString() : "none";

            return $"page:{request.PageNumber}:size:{request.PageSize}:order:{orderBy}:search:{searchTerm}:faculty:{facultyId}";
        }
    }
}