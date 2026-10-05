using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class ElectionTypeTestData
    {
        public static ElectionType CreateElectionType(int id = 1, string name = "Department Election",
            string? description = "An election conducted within a department.", bool active = true,
            int electionScopeId = 3)
        {
            return new ElectionType
            {
                Id = id,
                Name = name,
                Description = description,
                ElectionScopeId = electionScopeId,
                Active = active
            };
        }

        public static CreateElectionTypeRequest CreateElectionTypeRequest(string name = "Department Election",
            string? description = "An election conducted within a department.", int electionScopeId = 3)
        {
            return new CreateElectionTypeRequest
            {
                Name = name,
                Description = description,
                ElectionScopeId = electionScopeId
            };
        }

        public static CreateElectionTypeRequest CreateUpdateElectionTypeRequest(int id = 1,
            string name = "Updated Department Election",
            string? description = "An updated election type description.", int electionScopeId = 3)
        {
            return new CreateElectionTypeRequest
            {
                Id = id,
                Name = name,
                Description = description,
                ElectionScopeId = electionScopeId
            };
        }

        public static ElectionType CreateElectionTypeWithElectionData()
        {
            ElectionType electionType = CreateElectionType();

            Election election = new Election
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Department Election",
                YearId = 1,
                ElectionTypeId = electionType.Id,
                ElectionStatusId = 1,
                ElectionType = electionType
            };

            ElectionPosition firstElectionPosition = new ElectionPosition
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 1000,
                Currency = "NGN",
                Election = election
            };

            ElectionPosition secondElectionPosition = new ElectionPosition
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 1000,
                Currency = "NGN",
                Election = election
            };

            PositionApplication firstApplication = new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = Guid.NewGuid(),
                ElectionPositionId = firstElectionPosition.Id,
                PositionApplicationStatusId = 3,
                ElectionPosition = firstElectionPosition
            };

            PositionApplication secondApplication = new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = Guid.NewGuid(),
                ElectionPositionId = firstElectionPosition.Id,
                PositionApplicationStatusId = 3,
                ElectionPosition = firstElectionPosition
            };

            PositionApplication thirdApplication = new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = Guid.NewGuid(),
                ElectionPositionId = secondElectionPosition.Id,
                PositionApplicationStatusId = 2,
                ElectionPosition = secondElectionPosition
            };

            Contestant contestant = new Contestant
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = firstApplication.Id,
                PositionApplication = firstApplication
            };

            firstApplication.Contestant = contestant;

            firstElectionPosition.Applications.Add(firstApplication);
            firstElectionPosition.Applications.Add(secondApplication);
            secondElectionPosition.Applications.Add(thirdApplication);

            election.ElectionPositions.Add(firstElectionPosition);
            election.ElectionPositions.Add(secondElectionPosition);

            electionType.Elections.Add(election);

            return electionType;
        }
    }
}