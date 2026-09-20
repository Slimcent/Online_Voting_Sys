using Microsoft.AspNetCore.Identity;
using Moq;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Dtos.Response.Jwt;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;
using System.Security.Claims;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task UserLogin_UnknownEmail_ShouldRecordFailedLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            factory.SetLoginUser(null);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginFailed,
                ApplicationConstants.Audit.Outcomes.Failure, ApplicationConstants.Audit.Descriptions.InvalidCredentials,
                null, request.Email.Trim().ToLowerInvariant()), Times.Once);
        }

        [Fact]
        public async Task UserLogin_InactiveUser_ShouldRecordDeniedLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                Active = false
            };

            factory.SetLoginUser(user);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginFailed,
                ApplicationConstants.Audit.Outcomes.Denied, ApplicationConstants.Audit.Descriptions.InactiveAccount,
                user, null), Times.Once);
        }

        [Fact]
        public async Task UserLogin_LockedUser_ShouldRecordRejectedLockedLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                Active = true
            };

            factory.SetLoginUser(user);

            factory.UserManager.Setup(manager => manager.IsLockedOutAsync(user))
                .ReturnsAsync(true);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginRejectedLocked,
                ApplicationConstants.Audit.Outcomes.Denied, ApplicationConstants.Audit.Descriptions.LoginRejectedLocked,
                user, null), Times.Once);
        }

        [Fact]
        public async Task UserLogin_InvalidPassword_ShouldRecordFailedLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                Active = true
            };

            factory.SetLoginUser(user);

            factory.UserManager.Setup(manager => manager.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            factory.SignInManager.Setup(manager => manager.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(SignInResult.Failed);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginFailed,
                ApplicationConstants.Audit.Outcomes.Failure, ApplicationConstants.Audit.Descriptions.InvalidCredentials,
                user, null), Times.Once);
        }

        [Fact]
        public async Task UserLogin_LockoutTriggered_ShouldRecordAccountLocked()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                Active = true
            };

            factory.SetLoginUser(user);

            factory.UserManager.Setup(manager => manager.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            factory.SignInManager.Setup(manager => manager.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(SignInResult.LockedOut);

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.AccountLocked,
                ApplicationConstants.Audit.Outcomes.Denied, ApplicationConstants.Audit.Descriptions.AccountLocked,
                user, null), Times.Once);
        }

        [Fact]
        public async Task UserLogin_SuccessfulLogin_ShouldRecordSuccessfulLogin()
        {
            UserServiceFactory factory = new();

            LoginRequest request = LoginRequestFactory.CreateValid();

            User user = new()
            {
                Id = "user-id",
                Email = request.Email.Trim().ToLowerInvariant(),
                FirstName = "Test",
                LastName = "User",
                Active = true,
                UserType = new UserType
                {
                    Name = "Student"
                }
            };

            Role role = new()
            {
                Name = "Student"
            };

            JwtToken jwtToken = new()
            {
                Token = "access-token",
                Issuer = "test",
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };

            RefreshTokenResponse refreshTokenResponse = new()
            {
                RefreshToken = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            IList<string> roles = new List<string>
            {
                "Student"
            };

            IList<Claim> userClaims = new List<Claim>();
            IList<Claim> roleClaims = new List<Claim>();

            factory.SetLoginUser(user);

            factory.UserManager.Setup(manager => manager.IsLockedOutAsync(user))
                .ReturnsAsync(false);

            factory.SignInManager.Setup(manager => manager.CheckPasswordSignInAsync(user, request.Password, true))
                .ReturnsAsync(SignInResult.Success);

            factory.UserManager.Setup(manager => manager.GetRolesAsync(user))
                .ReturnsAsync(roles);

            factory.UserManager.Setup(manager => manager.GetClaimsAsync(user))
                .ReturnsAsync(userClaims);

            factory.RoleManager.Setup(manager => manager.FindByNameAsync("Student"))
                .ReturnsAsync(role);

            factory.RoleManager.Setup(manager => manager.GetClaimsAsync(role))
                .ReturnsAsync(roleClaims);

            factory.JwtAuthenticator.Setup(authenticator => authenticator.GenerateJwtToken(user, "Student", null, null))
                .ReturnsAsync(jwtToken);

            factory.RefreshTokenService.Setup(service => service.CreateRefreshToken(It.IsAny<RefreshTokenContext>()))
                .ReturnsAsync(Result<RefreshTokenResponse>.Success(refreshTokenResponse));

            Result<LoggedInUserResponse> result = await factory.Service.UserLogin(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(jwtToken, result.Value.JwtToken);
            Assert.Equal("Student", result.Value.UserType);
            Assert.Equal("Test User", result.Value.FullName);

            factory.AuditTrailService.Verify(service => service.RecordAuthenticationEvent(ApplicationConstants.Audit.Events.LoginSucceeded,
                ApplicationConstants.Audit.Outcomes.Success, ApplicationConstants.Audit.Descriptions.LoginSucceeded,
                user, null), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteUnconfirmedUsers_WithInvalidRetentionDays_ShouldThrowArgumentOutOfRangeException(int retentionDays)
        {
            using UserServiceFactory factory = new();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => factory.Service.DeleteUnconfirmedUsers(retentionDays));

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(), Times.Never);
            factory.UserRepository.Verify(repository => repository.DeleteRange(It.IsAny<IEnumerable<User>>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUnconfirmedUsers_WithNoMatchingUsers_ShouldReturnZero()
        {
            using UserServiceFactory factory = new();

            User confirmedUser = new()
            {
                Id = "confirmed-user",
                EmailConfirmed = true,
                Active = false,
                UserTypeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            User recentUnconfirmedUser = new()
            {
                Id = "recent-unconfirmed-user",
                EmailConfirmed = false,
                Active = true,
                UserTypeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            };

            await factory.AddCleanupData(new[] { confirmedUser, recentUnconfirmedUser });

            int result = await factory.Service.DeleteUnconfirmedUsers(5);

            Assert.Equal(0, result);

            factory.UserRepository.Verify(repository => repository.DeleteRange(It.IsAny<IEnumerable<User>>()), Times.Never);
            factory.StudentRepository.Verify(repository => repository.DeleteRange(It.IsAny<IEnumerable<Student>>()), Times.Never);
            factory.StaffRepository.Verify(repository => repository.DeleteRange(It.IsAny<IEnumerable<Staff>>()), Times.Never);
            factory.AddressRepository.Verify(repository => repository.DeleteRange(It.IsAny<IEnumerable<Address>>()), Times.Never);
            factory.RegisteredVoterRepository.Verify(repository => repository.DeleteRange(It.IsAny<IEnumerable<RegisteredVoter>>()), Times.Never);
            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteUnconfirmedUsers_WithExpiredUnconfirmedUsers_ShouldDeleteRelatedData()
        {
            using UserServiceFactory factory = new();

            User studentUser = new()
            {
                Id = "expired-student-user",
                EmailConfirmed = false,
                Active = true,
                UserTypeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            };

            User staffUser = new()
            {
                Id = "expired-staff-user",
                EmailConfirmed = false,
                Active = false,
                UserTypeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            };

            User confirmedUser = new()
            {
                Id = "confirmed-user",
                EmailConfirmed = true,
                Active = false,
                UserTypeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            };

            User recentUser = new()
            {
                Id = "recent-user",
                EmailConfirmed = false,
                Active = true,
                UserTypeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            };

            Student student = new()
            {
                Id = Guid.NewGuid(),
                UserId = studentUser.Id,
                DepartmentId = 1,
                GenderId = 1
            };

            Staff staff = new()
            {
                Id = Guid.NewGuid(),
                UserId = staffUser.Id,
                GenderId = 1
            };

            Address studentAddress = new()
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id
            };

            Address staffAddress = new()
            {
                Id = Guid.NewGuid(),
                StaffId = staff.Id
            };

            RegisteredVoter registeredVoter = new()
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                DepartmentId = 1
            };

            await factory.AddCleanupData(
                new[] { studentUser, staffUser, confirmedUser, recentUser },
                new[] { student },
                new[] { staff },
                new[] { studentAddress, staffAddress },
                new[] { registeredVoter });

            int result = await factory.Service.DeleteUnconfirmedUsers(5);

            Assert.Equal(2, result);

            factory.AddressRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<Address>>(items => items.Count() == 2
                    && items.Any(x => x.Id == studentAddress.Id)
                    && items.Any(x => x.Id == staffAddress.Id))), Times.Once);

            factory.RegisteredVoterRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<RegisteredVoter>>(items => items.Count() == 1
                    && items.Any(x => x.Id == registeredVoter.Id))), Times.Once);

            factory.StudentRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<Student>>(items => items.Count() == 1
                    && items.Any(x => x.Id == student.Id))), Times.Once);

            factory.StaffRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<Staff>>(items => items.Count() == 1
                    && items.Any(x => x.Id == staff.Id))), Times.Once);

            factory.UserRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<User>>(items => items.Count() == 2
                    && items.Any(x => x.Id == studentUser.Id)
                    && items.Any(x => x.Id == staffUser.Id)
                    && items.All(x => x.Id != confirmedUser.Id)
                    && items.All(x => x.Id != recentUser.Id))), Times.Once);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteUnconfirmedUsers_WithNoMatchingStudentOrStaff_ShouldDeleteUserSafely()
        {
            using UserServiceFactory factory = new();

            User user = new()
            {
                Id = "expired-user",
                EmailConfirmed = false,
                Active = true,
                UserTypeId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            };

            Student studentWithoutUser = new()
            {
                Id = Guid.NewGuid(),
                UserId = null,
                DepartmentId = 1,
                GenderId = 1
            };

            Staff staffWithoutUser = new()
            {
                Id = Guid.NewGuid(),
                UserId = null,
                GenderId = 1
            };

            await factory.AddCleanupData(new[] { user }, new[] { studentWithoutUser }, new[] { staffWithoutUser });

            int result = await factory.Service.DeleteUnconfirmedUsers(5);

            Assert.Equal(1, result);

            factory.StudentRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<Student>>(items => !items.Any())), Times.Once);

            factory.StaffRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<Staff>>(items => !items.Any())), Times.Once);

            factory.AddressRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<Address>>(items => !items.Any())), Times.Once);

            factory.RegisteredVoterRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<RegisteredVoter>>(items => !items.Any())), Times.Once);

            factory.UserRepository.Verify(repository => repository.DeleteRange(
                It.Is<IEnumerable<User>>(items => items.Count() == 1 && items.Any(x => x.Id == user.Id))), Times.Once);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteUnconfirmedUsers_WithUserAtRetentionBoundary_ShouldDeleteUser()
        {
            using UserServiceFactory factory = new();

            User user = UserTestData.CreateUserAtRetentionBoundary("boundary-user");

            await factory.AddCleanupData(new[] { user });

            int result = await factory.Service.DeleteUnconfirmedUsers(5);

            Assert.Equal(1, result);

            factory.UserRepository.Verify(repository => repository.DeleteRange(It.Is<IEnumerable<User>>(items => items.Count() == 1 && items.Any(x => x.Id == user.Id))), Times.Once);

            factory.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(), Times.Once);
        }
    }
}