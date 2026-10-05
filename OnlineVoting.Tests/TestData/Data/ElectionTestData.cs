using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class ElectionTestData
    {
        public static CreateElectionRequest CreateElectionRequest(string name = "2026 Department Election",
            long yearId = 1, int electionTypeId = 1, int electionStatusId = 1,
            long? facultyId = null, long? departmentId = 1,
            string? applicationStartAt = "2026-10-01T08:00:00",
            string? applicationEndAt = "2026-10-07T18:00:00",
            string? voterRegistrationStartAt = "2026-10-05T08:00:00",
            string? voterRegistrationEndAt = "2026-10-12T18:00:00",
            string? votingStartAt = "2026-10-15T08:00:00",
            string? votingEndAt = "2026-10-15T18:00:00")
        {
            return new CreateElectionRequest
            {
                Name = name,
                YearId = yearId,
                ElectionTypeId = electionTypeId,
                ElectionStatusId = electionStatusId,
                FacultyId = facultyId,
                DepartmentId = departmentId,
                ApplicationStartAt = applicationStartAt,
                ApplicationEndAt = applicationEndAt,
                VoterRegistrationStartAt = voterRegistrationStartAt,
                VoterRegistrationEndAt = voterRegistrationEndAt,
                VotingStartAt = votingStartAt,
                VotingEndAt = votingEndAt
            };
        }

        public static Election CreateElection(string name = "2026 Department Election", long yearId = 1, int electionTypeId = 1, int electionStatusId = 1,
            long? facultyId = null, long? departmentId = 1, bool active = true)
        {
            return new Election
            {
                Name = name,
                YearId = yearId,
                ElectionTypeId = electionTypeId,
                ElectionStatusId = electionStatusId,
                FacultyId = facultyId,
                DepartmentId = departmentId,
                Active = active
            };
        }

        public static List<Election> CreateElectionsForFiltering()
        {
            return new List<Election>
            {
                CreateElection(
                    name: "University Election 2026",
                    yearId: 1,
                    electionTypeId: 1,
                    electionStatusId: 1,
                    facultyId: null,
                    departmentId: null),

                CreateElection(
                    name: "Engineering Faculty Election 2026",
                    yearId: 1,
                    electionTypeId: 2,
                    electionStatusId: 2,
                    facultyId: 1,
                    departmentId: null),

                CreateElection(
                    name: "Computer Engineering Election 2026",
                    yearId: 1,
                    electionTypeId: 3,
                    electionStatusId: 2,
                    facultyId: null,
                    departmentId: 1),

                CreateElection(
                    name: "Science Faculty Election 2027",
                    yearId: 2,
                    electionTypeId: 2,
                    electionStatusId: 5,
                    facultyId: 2,
                    departmentId: null,
                    active: false)
            };
        }
    }
}
