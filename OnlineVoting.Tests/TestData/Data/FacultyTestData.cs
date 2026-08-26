using OnlineVoting.Models.Context;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class FacultyTestData
    {
        public static Faculty CreateFaculty(string name, bool activated = true)
        {
            return new Faculty
            {
                Name = name,
                Active = activated
            };
        }

        public static List<Faculty> CreateFaculties()
        {
            return new List<Faculty>
            {
                CreateFaculty("Engineering"),
                CreateFaculty("Arts", false),
                CreateFaculty("Science")
            };
        }

        public static async Task SeedFaculties(VotingDbContext context)
        {
            List<Faculty> faculties = CreateFaculties();

            await context.Faculties.AddRangeAsync(faculties);
            await context.SaveChangesAsync(true);
        }

        public static async Task SeedFacultyWithDepartments(VotingDbContext context)
        {
            Faculty faculty = CreateFaculty("Engineering");

            faculty.Departments.Add(new Department
            {
                Name = "Computer Engineering",
                Active = true
            });

            faculty.Departments.Add(new Department
            {
                Name = "Electrical Engineering",
                Active = true
            });

            await context.Faculties.AddAsync(faculty);
            await context.SaveChangesAsync(true);
        }

        public static CreateFacultyRequest CreateFacultyRequest(string? name = "Engineering", List<string>? names = null)
        {
            return new CreateFacultyRequest
            {
                Name = name,
                Names = names
            };
        }

        public static FacultyResponse CreateFacultyResponse(long id = 1, string name = "Engineering", bool active = true)
        {
            return new FacultyResponse
            {
                Id = id,
                Name = name,
                Active = active
            };
        }
    }
}