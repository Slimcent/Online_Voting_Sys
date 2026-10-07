using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;

namespace OnlineVoting.Tests.TestData
{
    public static class PositionApplicationTestData
    {
        public static User CreateUser(string? id = null, bool active = true)
        {
            return new User
            {
                Id = id ?? Guid.NewGuid().ToString(),
                FirstName = "John",
                LastName = "Doe",
                Email = $"john.doe.{Guid.NewGuid():N}@test.com",
                Active = active
            };
        }

        public static Student CreateStudent(User user, bool active = true)
        {
            return new Student
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                RegNumber = $"REG-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                DepartmentId = 1,
                GenderId = 1,
                Active = active
            };
        }

        public static Election CreateElection(bool active = true, DateTime? applicationStartAt = null, DateTime? applicationEndAt = null)
        {
            return new Election
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Election",
                Active = active,
                ApplicationStartAt = applicationStartAt ?? DateTime.UtcNow.AddDays(-1),
                ApplicationEndAt = applicationEndAt ?? DateTime.UtcNow.AddDays(1)
            };
        }

        public static ElectionPosition CreateElectionPosition(Election election, bool active = true, decimal applicationFee = 5000, string currency = "NGN")
        {
            return new ElectionPosition
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                Election = election,
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = applicationFee,
                Currency = currency,
                Active = active
            };
        }

        public static PositionApplicationStatus CreatePendingPaymentStatus(int id = 1)
        {
            return new PositionApplicationStatus
            {
                Id = id,
                Code = ApplicationConstants.PositionApplicationStatuses.PendingPayment,
                Name = "Pending Payment",
                Description = "Application is awaiting payment.",
                Active = true
            };
        }

        public static PositionApplication CreatePositionApplication(Student student, ElectionPosition electionPosition, PositionApplicationStatus status)
        {
            return new PositionApplication
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                Student = student,
                ElectionPositionId = electionPosition.Id,
                ElectionPosition = electionPosition,
                PositionApplicationStatusId = status.Id,
                PositionApplicationStatus = status,
                Active = true
            };
        }

        public static CreatePositionApplicationRequest CreateRequest(string? electionPositionId = null, string? idempotencyKey = null)
        {
            return new CreatePositionApplicationRequest
            {
                ElectionPositionId = electionPositionId ?? Guid.NewGuid().ToString(),
                IdempotencyKey = idempotencyKey ?? Guid.NewGuid().ToString()
            };
        }

        public static InvoiceResponse CreateInvoiceResponse(decimal amount = 5000, string currency = "NGN")
        {
            return new InvoiceResponse
            {
                Id = Guid.NewGuid().ToString(),
                InvoiceNumber = $"INV-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
                Amount = amount,
                Currency = currency,
                Status = "Unpaid"
            };
        }

        public static CreatePositionApplicationResponse CreatePositionApplicationResponse(string? positionApplicationId = null, decimal amount = 5000,
            string currency = "NGN")
        {
            return new CreatePositionApplicationResponse
            {
                PositionApplicationId = positionApplicationId ?? Guid.NewGuid().ToString(),
                InvoiceId = Guid.NewGuid().ToString(),
                InvoiceNumber = $"INV-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
                Amount = amount,
                Currency = currency,
                ApplicationStatus = "Pending Payment",
                InvoiceStatus = "Unpaid"
            };
        }

        public static IdempotencyRecord CreateIdempotencyRecord(string userId, string idempotencyKey, string requestHash, string status)
        {
            return new IdempotencyRecord
            {
                Id = Guid.NewGuid().ToString(),
                Key = idempotencyKey,
                UserId = userId,
                Operation = ApplicationConstants.IdempotencyOperations.CreatePositionApplication,
                RequestHash = requestHash,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Role CreateRole(string roleName)
        {
            return new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            };
        }

        public static ApplicationUserRole CreateApplicationUserRole(User user, Role role, bool active = true)
        {
            return new ApplicationUserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                User = user,
                Role = role,
                Active = active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}