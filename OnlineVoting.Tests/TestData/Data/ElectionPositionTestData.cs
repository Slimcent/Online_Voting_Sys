using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData.Data
{
    public static class ElectionPositionTestData
    {
        public static Position CreatePosition(string name = "President", bool active = true)
        {
            return new Position
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Active = active
            };
        }

        public static ElectionPosition CreateElectionPosition(Election election, Position position,
            decimal applicationFee = 1000, string currency = "NGN", bool active = true)
        {
            return new ElectionPosition
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = position.Id,
                ApplicationFee = applicationFee,
                Currency = currency,
                Active = active,
                Election = election,
                Position = position
            };
        }

        public static PositionApplication CreatePositionApplication(ElectionPosition electionPosition,
            int positionApplicationStatusId = 1, bool active = true)
        {
            return new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = Guid.NewGuid(),
                ElectionPositionId = electionPosition.Id,
                PositionApplicationStatusId = positionApplicationStatusId,
                Active = active,
                ElectionPosition = electionPosition
            };
        }

        public static User CreateUser(string firstName = "John", string lastName = "Doe")
        {
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = firstName,
                LastName = lastName,
                UserTypeId = 1,
                Active = true
            };
        }

        public static Student CreateStudent(User user, Department department, string regNumber = "CE/2026/001")
        {
            return new Student
            {
                Id = Guid.NewGuid(),
                RegNumber = regNumber,
                UserId = user.Id,
                User = user,
                DepartmentId = department.Id,
                Department = department,
                GenderId = 1,
                Active = true
            };
        }

        public static PositionApplicationStatus CreatePositionApplicationStatus(int id = 3,
            string name = "Approved", bool active = true)
        {
            return new PositionApplicationStatus
            {
                Id = id,
                Name = name,
                Active = active
            };
        }

        public static PositionApplication CreatePositionApplication(ElectionPosition electionPosition, Student student,
            PositionApplicationStatus positionApplicationStatus, bool active = true)
        {
            return new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                Student = student,
                ElectionPositionId = electionPosition.Id,
                ElectionPosition = electionPosition,
                PositionApplicationStatusId = positionApplicationStatus.Id,
                PositionApplicationStatus = positionApplicationStatus,
                Active = active
            };
        }

        public static CreateElectionPositionRequest CreateElectionPositionRequest(string? electionId = null,
            string? positionId = null, decimal applicationFee = 1000, string currency = "NGN")
        {
            return new CreateElectionPositionRequest
            {
                ElectionId = electionId ?? Guid.NewGuid().ToString(),
                PositionId = positionId ?? Guid.NewGuid().ToString(),
                ApplicationFee = applicationFee,
                Currency = currency
            };
        }

        public static Election CreateElection(string id)
        {
            return new Election
            {
                Id = id,
                Name = "2026 Department Election",
                YearId = 1,
                ElectionTypeId = 1,
                ElectionStatusId = 1,
                DepartmentId = 1
            };
        }

        public static Position CreatePositionWithId(string id, string name = "President", bool active = true)
        {
            return new Position
            {
                Id = id,
                Name = name,
                Active = active
            };
        }

        public static CreateElectionPositionsRequest CreateElectionPositionsRequest(string? electionId = null, int numberOfPositions = 3, 
                decimal applicationFee = 1000, string currency = "NGN")
        {
            List<CreateElectionPositionItemRequest> electionPositions = new();

            for (int i = 0; i < numberOfPositions; i++)
            {
                electionPositions.Add(new CreateElectionPositionItemRequest
                {
                    PositionId = Guid.NewGuid().ToString(),
                    ApplicationFee = applicationFee,
                    Currency = currency
                });
            }

            return new CreateElectionPositionsRequest
            {
                ElectionId = electionId ?? Guid.NewGuid().ToString(),
                ElectionPositions = electionPositions
            };
        }
    }
}