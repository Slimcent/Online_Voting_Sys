using Moq;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Caching.Tags;
using OnlineVoting.Tests.Factories;
using OnlineVoting.Tests.TestData;
using Microsoft.EntityFrameworkCore;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;

namespace OnlineVoting.Tests.Services
{
    public class ContestantServiceTests
    {
        [Fact]
        public async Task GetContestants_ReturnsContestants()
        {
            using ContestantServiceFactory factory = new ContestantServiceFactory();

            User user = PositionApplicationTestData.CreateUser();
            user.FirstName = "Obinna";
            user.LastName = "Achara";

            Student student = PositionApplicationTestData.CreateStudent(user);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position position = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            ElectionPosition electionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = position.Id,
                Election = election,
                Position = position
            };

            PositionApplication positionApplication = PositionApplicationTestData.CreatePositionApplication(
                student, electionPosition, approvedStatus);

            Contestant contestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                PositionApplication = positionApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            positionApplication.Contestant = contestant;

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.Add(position);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Contestants.Add(contestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<ContestantResponse>>(It.IsAny<PagedList<Contestant>>()))
                .Returns((PagedList<Contestant> contestants) => new PagedResponse<ContestantResponse>
                {
                    Items = contestants.Select(item => new ContestantResponse
                    {
                        ContestantId = item.Id,
                        PositionApplicationId = item.PositionApplicationId,
                        ContestantName = item.PositionApplication.Student.User != null
                            ? $"{item.PositionApplication.Student.User.FirstName} {item.PositionApplication.Student.User.LastName}".Trim()
                            : string.Empty,
                        RegistrationNumber = item.PositionApplication.Student.RegNumber,
                        ElectionName = item.PositionApplication.ElectionPosition.Election.Name,
                        PositionName = item.PositionApplication.ElectionPosition.Position.Name,
                        Active = item.Active,
                        CreatedAt = item.CreatedAt
                    }).ToList(),
                    MetaData = contestants.MetaData
                });

            ContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ContestantResponse>> result = await factory.Service.GetContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            ContestantResponse response = result.Value.Items.Single();

            Assert.Equal(contestant.Id, response.ContestantId);
            Assert.Equal(positionApplication.Id, response.PositionApplicationId);
            Assert.Equal("Obinna Achara", response.ContestantName);
            Assert.Equal(election.Name, response.ElectionName);
            Assert.Equal(position.Name, response.PositionName);
            Assert.True(response.Active);

            factory.CacheService.Verify(x => x.GetOrCreate(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<ContestantResponse>>>>(),
                It.IsAny<CacheEntryOptions?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
                
        [Fact]
        public async Task GetContestants_WithPositionId_ShouldReturnFilteredContestants()
        {
            using ContestantServiceFactory factory = new ContestantServiceFactory();

            User firstUser = PositionApplicationTestData.CreateUser();
            User secondUser = PositionApplicationTestData.CreateUser();

            Student firstStudent = PositionApplicationTestData.CreateStudent(firstUser);
            Student secondStudent = PositionApplicationTestData.CreateStudent(secondUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position presidentPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secretaryPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition presidentElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = presidentPosition.Id,
                Election = election,
                Position = presidentPosition
            };

            ElectionPosition secretaryElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secretaryPosition.Id,
                Election = election,
                Position = secretaryPosition
            };

            PositionApplication presidentApplication = PositionApplicationTestData.CreatePositionApplication(
                firstStudent, presidentElectionPosition, approvedStatus);

            PositionApplication secretaryApplication = PositionApplicationTestData.CreatePositionApplication(
                secondStudent, secretaryElectionPosition, approvedStatus);

            Contestant presidentContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = presidentApplication.Id,
                PositionApplication = presidentApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Contestant secretaryContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = secretaryApplication.Id,
                PositionApplication = secretaryApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            presidentApplication.Contestant = presidentContestant;
            secretaryApplication.Contestant = secretaryContestant;

            factory.DbContextFactory.Context.Users.AddRange(firstUser, secondUser);
            factory.DbContextFactory.Context.Students.AddRange(firstStudent, secondStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(presidentPosition, secretaryPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(presidentElectionPosition, secretaryElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(presidentApplication, secretaryApplication);
            factory.DbContextFactory.Context.Contestants.AddRange(presidentContestant, secretaryContestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<ContestantResponse>>(It.IsAny<PagedList<Contestant>>()))
                .Returns((PagedList<Contestant> contestants) => new PagedResponse<ContestantResponse>
                {
                    Items = contestants.Select(item => new ContestantResponse
                    {
                        ContestantId = item.Id,
                        PositionApplicationId = item.PositionApplicationId,
                        ElectionName = item.PositionApplication.ElectionPosition.Election.Name,
                        PositionName = item.PositionApplication.ElectionPosition.Position.Name,
                        Active = item.Active
                    }).ToList(),
                    MetaData = contestants.MetaData
                });

            ContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                PositionId = presidentPosition.Id
            };

            Result<PagedResponse<ContestantResponse>> result = await factory.Service.GetContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(presidentContestant.Id, result.Value.Items.Single().ContestantId);
            Assert.Equal("President", result.Value.Items.Single().PositionName);
        }

        [Fact]
        public async Task GetContestants_WithActive_ShouldReturnFilteredContestants()
        {
            using ContestantServiceFactory factory = new ContestantServiceFactory();

            User firstUser = PositionApplicationTestData.CreateUser();
            User secondUser = PositionApplicationTestData.CreateUser();

            Student firstStudent = PositionApplicationTestData.CreateStudent(firstUser);
            Student secondStudent = PositionApplicationTestData.CreateStudent(secondUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = firstPosition.Id,
                Election = election,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secondPosition.Id,
                Election = election,
                Position = secondPosition
            };

            PositionApplication firstApplication = PositionApplicationTestData.CreatePositionApplication(
                firstStudent, firstElectionPosition, approvedStatus);

            PositionApplication secondApplication = PositionApplicationTestData.CreatePositionApplication(
                secondStudent, secondElectionPosition, approvedStatus);

            Contestant activeContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = firstApplication.Id,
                PositionApplication = firstApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Contestant inactiveContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = secondApplication.Id,
                PositionApplication = secondApplication,
                Active = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            firstApplication.Contestant = activeContestant;
            secondApplication.Contestant = inactiveContestant;

            factory.DbContextFactory.Context.Users.AddRange(firstUser, secondUser);
            factory.DbContextFactory.Context.Students.AddRange(firstStudent, secondStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(firstApplication, secondApplication);
            factory.DbContextFactory.Context.Contestants.AddRange(activeContestant, inactiveContestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<ContestantResponse>>(It.IsAny<PagedList<Contestant>>()))
                .Returns((PagedList<Contestant> contestants) => new PagedResponse<ContestantResponse>
                {
                    Items = contestants.Select(item => new ContestantResponse
                    {
                        ContestantId = item.Id,
                        PositionApplicationId = item.PositionApplicationId,
                        PositionName = item.PositionApplication.ElectionPosition.Position.Name,
                        Active = item.Active
                    }).ToList(),
                    MetaData = contestants.MetaData
                });

            ContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                Active = true
            };

            Result<PagedResponse<ContestantResponse>> result = await factory.Service.GetContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            ContestantResponse response = result.Value.Items.Single();

            Assert.Equal(activeContestant.Id, response.ContestantId);
            Assert.True(response.Active);
        }

        [Fact]
        public async Task GetContestants_WithSearchTerm_ShouldReturnMatchingContestants()
        {
            using ContestantServiceFactory factory = new ContestantServiceFactory();

            User matchingUser = PositionApplicationTestData.CreateUser();
            matchingUser.FirstName = "Obinna";
            matchingUser.LastName = "Achara";

            User nonMatchingUser = PositionApplicationTestData.CreateUser();
            nonMatchingUser.FirstName = "John";
            nonMatchingUser.LastName = "Doe";

            Student matchingStudent = PositionApplicationTestData.CreateStudent(matchingUser);
            Student nonMatchingStudent = PositionApplicationTestData.CreateStudent(nonMatchingUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position presidentPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secretaryPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition presidentElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = presidentPosition.Id,
                Election = election,
                Position = presidentPosition
            };

            ElectionPosition secretaryElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secretaryPosition.Id,
                Election = election,
                Position = secretaryPosition
            };

            PositionApplication matchingApplication = PositionApplicationTestData.CreatePositionApplication(
                matchingStudent, presidentElectionPosition, approvedStatus);

            PositionApplication nonMatchingApplication = PositionApplicationTestData.CreatePositionApplication(
                nonMatchingStudent, secretaryElectionPosition, approvedStatus);

            Contestant matchingContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = matchingApplication.Id,
                PositionApplication = matchingApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Contestant nonMatchingContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = nonMatchingApplication.Id,
                PositionApplication = nonMatchingApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            matchingApplication.Contestant = matchingContestant;
            nonMatchingApplication.Contestant = nonMatchingContestant;

            factory.DbContextFactory.Context.Users.AddRange(matchingUser, nonMatchingUser);
            factory.DbContextFactory.Context.Students.AddRange(matchingStudent, nonMatchingStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(presidentPosition, secretaryPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(presidentElectionPosition, secretaryElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(matchingApplication, nonMatchingApplication);
            factory.DbContextFactory.Context.Contestants.AddRange(matchingContestant, nonMatchingContestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<ContestantResponse>>(It.IsAny<PagedList<Contestant>>()))
                .Returns((PagedList<Contestant> contestants) => new PagedResponse<ContestantResponse>
                {
                    Items = contestants.Select(item => new ContestantResponse
                    {
                        ContestantId = item.Id,
                        PositionApplicationId = item.PositionApplicationId,
                        ContestantName = item.PositionApplication.Student.User != null
                            ? $"{item.PositionApplication.Student.User.FirstName} {item.PositionApplication.Student.User.LastName}".Trim()
                            : string.Empty,
                        ElectionName = item.PositionApplication.ElectionPosition.Election.Name,
                        PositionName = item.PositionApplication.ElectionPosition.Position.Name,
                        Active = item.Active
                    }).ToList(),
                    MetaData = contestants.MetaData
                });

            ContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Obinna"
            };

            Result<PagedResponse<ContestantResponse>> result = await factory.Service.GetContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            ContestantResponse response = result.Value.Items.Single();

            Assert.Equal(matchingContestant.Id, response.ContestantId);
            Assert.Equal("Obinna Achara", response.ContestantName);
            Assert.Equal("President", response.PositionName);
        }

        [Fact]
        public async Task GetContestants_WithElectionId_ShouldReturnFilteredContestants()
        {
            using ContestantServiceFactory factory = new ContestantServiceFactory();

            User firstUser = PositionApplicationTestData.CreateUser();
            User secondUser = PositionApplicationTestData.CreateUser();

            Student firstStudent = PositionApplicationTestData.CreateStudent(firstUser);
            Student secondStudent = PositionApplicationTestData.CreateStudent(secondUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election firstElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Election secondElection = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Science Election",
                ElectionTypeId = 2,
                YearId = 1
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = firstElection.Id,
                PositionId = firstPosition.Id,
                Election = firstElection,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = secondElection.Id,
                PositionId = secondPosition.Id,
                Election = secondElection,
                Position = secondPosition
            };

            PositionApplication firstApplication = PositionApplicationTestData.CreatePositionApplication(
                firstStudent, firstElectionPosition, approvedStatus);

            PositionApplication secondApplication = PositionApplicationTestData.CreatePositionApplication(
                secondStudent, secondElectionPosition, approvedStatus);

            Contestant firstContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = firstApplication.Id,
                PositionApplication = firstApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Contestant secondContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = secondApplication.Id,
                PositionApplication = secondApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            firstApplication.Contestant = firstContestant;
            secondApplication.Contestant = secondContestant;

            factory.DbContextFactory.Context.Users.AddRange(firstUser, secondUser);
            factory.DbContextFactory.Context.Students.AddRange(firstStudent, secondStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.AddRange(firstElection, secondElection);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(firstApplication, secondApplication);
            factory.DbContextFactory.Context.Contestants.AddRange(firstContestant, secondContestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<ContestantResponse>>(It.IsAny<PagedList<Contestant>>()))
                .Returns((PagedList<Contestant> contestants) => new PagedResponse<ContestantResponse>
                {
                    Items = contestants.Select(item => new ContestantResponse
                    {
                        ContestantId = item.Id,
                        PositionApplicationId = item.PositionApplicationId,
                        ElectionName = item.PositionApplication.ElectionPosition.Election.Name,
                        PositionName = item.PositionApplication.ElectionPosition.Position.Name,
                        Active = item.Active
                    }).ToList(),
                    MetaData = contestants.MetaData
                });

            ContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = firstElection.Id
            };

            Result<PagedResponse<ContestantResponse>> result = await factory.Service.GetContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(firstContestant.Id, result.Value.Items.Single().ContestantId);
            Assert.Equal(firstElection.Name, result.Value.Items.Single().ElectionName);
        }

        [Fact]
        public async Task GetContestants_WithElectionPositionId_ShouldReturnFilteredContestants()
        {
            using ContestantServiceFactory factory = new ContestantServiceFactory();

            User firstUser = PositionApplicationTestData.CreateUser();
            User secondUser = PositionApplicationTestData.CreateUser();

            Student firstStudent = PositionApplicationTestData.CreateStudent(firstUser);
            Student secondStudent = PositionApplicationTestData.CreateStudent(secondUser);

            PositionApplicationStatus approvedStatus = new()
            {
                Id = 3,
                Code = ApplicationConstants.PositionApplicationStatuses.Approved,
                Name = "Approved",
                Active = true
            };

            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "2026 Engineering Election",
                ElectionTypeId = 1,
                YearId = 1
            };

            Position firstPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "President",
                Active = true
            };

            Position secondPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Secretary",
                Active = true
            };

            ElectionPosition firstElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = firstPosition.Id,
                Election = election,
                Position = firstPosition
            };

            ElectionPosition secondElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = secondPosition.Id,
                Election = election,
                Position = secondPosition
            };

            PositionApplication firstApplication = PositionApplicationTestData.CreatePositionApplication(
                firstStudent, firstElectionPosition, approvedStatus);

            PositionApplication secondApplication = PositionApplicationTestData.CreatePositionApplication(
                secondStudent, secondElectionPosition, approvedStatus);

            Contestant firstContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = firstApplication.Id,
                PositionApplication = firstApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Contestant secondContestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = secondApplication.Id,
                PositionApplication = secondApplication,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            firstApplication.Contestant = firstContestant;
            secondApplication.Contestant = secondContestant;

            factory.DbContextFactory.Context.Users.AddRange(firstUser, secondUser);
            factory.DbContextFactory.Context.Students.AddRange(firstStudent, secondStudent);
            factory.DbContextFactory.Context.PositionApplicationStatuses.Add(approvedStatus);
            factory.DbContextFactory.Context.Elections.Add(election);
            factory.DbContextFactory.Context.Positions.AddRange(firstPosition, secondPosition);
            factory.DbContextFactory.Context.ElectionPositions.AddRange(firstElectionPosition, secondElectionPosition);
            factory.DbContextFactory.Context.PositionApplications.AddRange(firstApplication, secondApplication);
            factory.DbContextFactory.Context.Contestants.AddRange(firstContestant, secondContestant);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<ContestantResponse>>(It.IsAny<PagedList<Contestant>>()))
                .Returns((PagedList<Contestant> contestants) => new PagedResponse<ContestantResponse>
                {
                    Items = contestants.Select(item => new ContestantResponse
                    {
                        ContestantId = item.Id,
                        PositionApplicationId = item.PositionApplicationId,
                        ElectionName = item.PositionApplication.ElectionPosition.Election.Name,
                        PositionName = item.PositionApplication.ElectionPosition.Position.Name,
                        Active = item.Active
                    }).ToList(),
                    MetaData = contestants.MetaData
                });

            ContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionPositionId = firstElectionPosition.Id
            };

            Result<PagedResponse<ContestantResponse>> result = await factory.Service.GetContestants(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);
            Assert.Equal(firstContestant.Id, result.Value.Items.Single().ContestantId);
            Assert.Equal("President", result.Value.Items.Single().PositionName);
        }

        [Fact]
        public async Task ToggleContestantActivation_ActivatesInactiveContestant()
        {
            using ContestantServiceFactory factory = new ContestantServiceFactory();

            Contestant contestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = Guid.NewGuid().ToString(),
                Active = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            factory.DbContextFactory.Context.Contestants.Add(contestant);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            Result<string> result = await factory.Service.ToggleContestantActivation(contestant.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Contestant activated successfully.", result.Value);

            Contestant? updatedContestant = await factory.DbContextFactory.Context.Contestants
                .FirstOrDefaultAsync(x => x.Id == contestant.Id);

            Assert.NotNull(updatedContestant);
            Assert.True(updatedContestant.Active);

            factory.ContestantRepository.Verify(x => x.UpdateAsync(It.Is<Contestant>(x => x.Id == contestant.Id && x.Active), It.IsAny<bool>()), Times.Once);
            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.Contestant, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleContestantActivation_DeactivatesActiveContestant()
        {
            using ContestantServiceFactory factory = new ContestantServiceFactory();

            Contestant contestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = Guid.NewGuid().ToString(),
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            factory.DbContextFactory.Context.Contestants.Add(contestant);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            Result<string> result = await factory.Service.ToggleContestantActivation(contestant.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Contestant deactivated successfully.", result.Value);

            Contestant? updatedContestant = await factory.DbContextFactory.Context.Contestants
                .FirstOrDefaultAsync(x => x.Id == contestant.Id);

            Assert.NotNull(updatedContestant);
            Assert.False(updatedContestant.Active);

            factory.ContestantRepository.Verify(x => x.UpdateAsync(It.Is<Contestant>(x => x.Id == contestant.Id && !x.Active), It.IsAny<bool>()), Times.Once);
            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.Contestant, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleContestantActivation_ReturnsNotFound_WhenContestantDoesNotExist()
        {
            using ContestantServiceFactory factory = new ContestantServiceFactory();

            string contestantId = Guid.NewGuid().ToString();

            Result<string> result = await factory.Service.ToggleContestantActivation(contestantId);

            Assert.Equal(ResultStatus.NotFound, result.Status);

            factory.ContestantRepository.Verify(x => x.UpdateAsync(It.IsAny<Contestant>(), It.IsAny<bool>()), Times.Never);
            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.Contestant, It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}