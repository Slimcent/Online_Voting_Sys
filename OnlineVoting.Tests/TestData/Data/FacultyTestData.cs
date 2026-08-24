using OnlineVoting.Models.Context;
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
                Activated = activated
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
                Activated = true
            });

            faculty.Departments.Add(new Department
            {
                Name = "Electrical Engineering",
                Activated = true
            });

            await context.Faculties.AddAsync(faculty);
            await context.SaveChangesAsync(true);
        }
    }
}