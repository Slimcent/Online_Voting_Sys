using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class ElectionStatusTestData
    {
        public static ElectionStatus CreateElectionStatus(int id = 1,
            string code = ApplicationConstants.ElectionStatusCodes.Draft, string name = "Draft",
            string? description = "The election is still being configured.", bool active = true)
        {
            return new ElectionStatus
            {
                Id = id,
                Code = code,
                Name = name,
                Description = description,
                Active = active
            };
        }

        public static List<ElectionStatus> CreateElectionStatuses()
        {
            return new List<ElectionStatus>
            {
                CreateElectionStatus(1, ApplicationConstants.ElectionStatusCodes.Draft, "Draft"),
                CreateElectionStatus(2, ApplicationConstants.ElectionStatusCodes.RegistrationOpen, "Registration Open"),
                CreateElectionStatus(3, ApplicationConstants.ElectionStatusCodes.RegistrationClosed, "Registration Closed"),
                CreateElectionStatus(4, ApplicationConstants.ElectionStatusCodes.VotingOpen, "Voting Open"),
                CreateElectionStatus(5, ApplicationConstants.ElectionStatusCodes.Completed, "Completed"),
                CreateElectionStatus(6, ApplicationConstants.ElectionStatusCodes.Cancelled, "Cancelled")
            };
        }

        public static UpdateElectionStatusRequest CreateUpdateElectionStatusRequest(int id = 1,
            string name = "Updated Draft", string? description = "Updated election status description.")
        {
            return new UpdateElectionStatusRequest
            {
                Id = id,
                Name = name,
                Description = description
            };
        }

        public static ElectionStatus CreateElectionStatusWithElectionData()
        {
            ElectionStatus electionStatus = CreateElectionStatus();

            Election firstElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "First Election",
                YearId = 1,
                ElectionTypeId = 1,
                ElectionStatusId = electionStatus.Id,
                ElectionStatus = electionStatus
            };

            Election secondElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Second Election",
                YearId = 1,
                ElectionTypeId = 1,
                ElectionStatusId = electionStatus.Id,
                ElectionStatus = electionStatus
            };

            electionStatus.Elections.Add(firstElection);
            electionStatus.Elections.Add(secondElection);

            return electionStatus;
        }
    }
}