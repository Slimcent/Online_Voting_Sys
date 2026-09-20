using OnlineVoting.Models.Dtos.Request;

namespace OnlineVoting.Services.Caching.Keys
{
    public static class FacultyCacheKeys
    {
        private const string Prefix = "onlinevoting:v1:faculty";

        public static string GetFaculty(long id)
        {
            return $"{Prefix}:id:{id}";
        }

        public static string GetFacultyWithDepartments(long id)
        {
            return $"{Prefix}:id:{id}:departments";
        }

        public static string GetFaculties(FacultyRequestParameters request)
        {
            return BuildListKey("list", request);
        }

        public static string GetFacultiesWithDepartments(FacultyRequestParameters request)
        {
            return BuildListKey("list:departments", request);
        }

        private static string BuildListKey(string type, FacultyRequestParameters request)
        {
            string searchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? "none" : Uri.EscapeDataString(request.SearchTerm.Trim());
            string orderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "none" : Uri.EscapeDataString(request.OrderBy.Trim());

            return $"{Prefix}:{type}:page:{request.PageNumber}:size:{request.PageSize}:order:{orderBy}:search:{searchTerm}";
        }
    }
}