using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class DepartmentTestData
    {
        public static Department CreateDepartment(string name = "Computer Engineering", long facultyId = 1, bool active = true)
        {
            return new Department
            {
                Name = name,
                FacultyId = facultyId,
                Active = active
            };
        }

        public static List<Department> CreateDepartments(long facultyId = 1)
        {
            return new List<Department>
            {
                CreateDepartment("Computer Engineering", facultyId),
                CreateDepartment("Electrical Engineering", facultyId),
                CreateDepartment("Mechanical Engineering", facultyId)
            };
        }

        public static DepartmentResponse CreateDepartmentResponse(long id = 1, string name = "Computer Engineering", long facultyId = 1, bool active = true)
        {
            return new DepartmentResponse
            {
                Id = id,
                Name = name,
                FacultyId = facultyId,
                Active = active
            };
        }
    }
}