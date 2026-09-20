using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Exceptions;
using VotingSystem.Logger;

namespace OnlineVoting.Services.Extension
{
    public static class StudentUploadExtension
    {
        public static void SetDepartmentIds(this List<Dictionary<string, string>> studentData, Dictionary<string, long> departmentDictionary)
        {
            foreach (Dictionary<string, string> student in studentData)
            {
                if (!student.TryGetValue("Department", out string? departmentName) || string.IsNullOrWhiteSpace(departmentName))
                    throw new InvalidDataException("Department is required.");

                string normalizedDepartment = departmentName.NormalizeLookupValue();

                if (!departmentDictionary.TryGetValue(normalizedDepartment, out long departmentId))
                    throw new InvalidDataException($"Department {departmentName} does not exist.");

                student["DepartmentId"] = departmentId.ToString();
            }
        }

        public static void SetGenderIds(this List<Dictionary<string, string>> studentData, Dictionary<string, int> genderDictionary)
        {
            foreach (Dictionary<string, string> student in studentData)
            {
                if (!student.TryGetValue("Gender", out string? genderName) || string.IsNullOrWhiteSpace(genderName))
                    throw new InvalidDataException("Gender is required.");

                string normalizedGender = genderName.NormalizeGender();

                if (!genderDictionary.TryGetValue(normalizedGender, out int genderId))
                    throw new InvalidDataException($"Gender {genderName} does not exist.");

                student["GenderId"] = genderId.ToString();
            }
        }

        public static void ValidateDepartmentIds(this IEnumerable<CreateStudentRequest> studentsToUpload, IEnumerable<Department> departments, ILoggerMessage loggerMessage)
        {
            HashSet<long> departmentIds = departments.Select(department => department.Id).ToHashSet();

            CreateStudentRequest? invalidStudent = studentsToUpload.FirstOrDefault(student => !departmentIds.Contains(student.DepartmentId));

            if (invalidStudent != null)
            {
                loggerMessage.LogWarn($"Student bulk upload failed because department with ID {invalidStudent.DepartmentId} does not exist.");

                throw new InvalidDataException($"Department with ID {invalidStudent.DepartmentId} does not exist.");
            }
        }

        public static void ValidateGenderIds(this IEnumerable<CreateStudentRequest> studentsToUpload, IEnumerable<Gender> genders, ILoggerMessage loggerMessage)
        {
            HashSet<int> genderIds = genders.Select(gender => gender.Id).ToHashSet();

            CreateStudentRequest? invalidStudent = studentsToUpload.FirstOrDefault(student => !genderIds.Contains(student.GenderId));

            if (invalidStudent != null)
            {
                loggerMessage.LogWarn($"Student bulk upload failed because gender with ID {invalidStudent.GenderId} does not exist.");

                throw new InvalidDataException($"Gender with ID {invalidStudent.GenderId} does not exist.");
            }
        }
    }
}