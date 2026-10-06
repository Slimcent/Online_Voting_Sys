using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Caching.Configuration;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Request.Email;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.BackgroundTasks;
using OnlineVoting.Services.Caching.Policies;
using OnlineVoting.Services.Caching.Tags;
using OnlineVoting.Tests.Factories;
using System.Linq.Expressions;

namespace OnlineVoting.Tests.Services
{
    public class VoterServiceTests
    {
        [Fact]
        public async Task RegisterVoter_ReturnsCreated_WhenRegistrationIsSuccessful()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory);

            SetupRegisteredVoterMapping(factory, student, election);

            RegisterVoterRequest request = new()
            {
                RegNumber = student.RegNumber!,
                ElectionId = election.Id
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(student.Id, result.Value.StudentId);
            Assert.Equal(student.RegNumber, result.Value.RegistrationNumber);
            Assert.Equal($"{student.User!.FirstName} {student.User.LastName}", result.Value.StudentName);
            Assert.Equal(election.Id, result.Value.ElectionId);
            Assert.Equal(election.Name, result.Value.ElectionName);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.VotingCode));
            Assert.True(result.Value.Active);

            RegisteredVoter? registeredVoter = factory.DbContextFactory.Context.RegisteredVoters
                .FirstOrDefault(x => x.StudentId == student.Id && x.ElectionId == election.Id);

            Assert.NotNull(registeredVoter);
            Assert.Equal(student.Id, registeredVoter.StudentId);
            Assert.Equal(election.Id, registeredVoter.ElectionId);
            Assert.False(string.IsNullOrWhiteSpace(registeredVoter.VotingCode));

            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.RegisteredVoter, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterVoter_ReturnsNotFound_WhenStudentDoesNotExist()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Election election = await CreateElection(factory);

            RegisterVoterRequest request = new()
            {
                RegNumber = "UNKNOWN",
                ElectionId = election.Id
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Student with registration number UNKNOWN was not found.", result.Error);

            factory.RegisteredVoterRepository.Verify(x => x.AddAsync(It.IsAny<RegisteredVoter>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RegisterVoter_ReturnsNotFound_WhenElectionDoesNotExist()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            string electionId = Guid.NewGuid().ToString();

            RegisterVoterRequest request = new()
            {
                RegNumber = student.RegNumber!,
                ElectionId = electionId
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal($"Election with id {electionId} was not found.", result.Error);

            factory.RegisteredVoterRepository.Verify(x => x.AddAsync(It.IsAny<RegisteredVoter>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RegisterVoter_ReturnsConflict_WhenElectionIsInactive()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory, active: false);

            RegisterVoterRequest request = new()
            {
                RegNumber = student.RegNumber!,
                ElectionId = election.Id
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Voter registration is not available for this election.", result.Error);

            factory.RegisteredVoterRepository.Verify(x => x.AddAsync(It.IsAny<RegisteredVoter>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RegisterVoter_ReturnsConflict_WhenRegistrationHasNotStarted()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory);
            election.VoterRegistrationStartAt = DateTime.UtcNow.AddHours(1);
            election.VoterRegistrationEndAt = DateTime.UtcNow.AddHours(2);

            factory.DbContextFactory.Context.Elections.Update(election);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            RegisterVoterRequest request = new()
            {
                RegNumber = student.RegNumber!,
                ElectionId = election.Id
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Voter registration has not started for this election.", result.Error);

            factory.RegisteredVoterRepository.Verify(x => x.AddAsync(It.IsAny<RegisteredVoter>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RegisterVoter_ReturnsConflict_WhenRegistrationHasEnded()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory);
            election.VoterRegistrationStartAt = DateTime.UtcNow.AddHours(-2);
            election.VoterRegistrationEndAt = DateTime.UtcNow.AddHours(-1);

            factory.DbContextFactory.Context.Elections.Update(election);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            RegisterVoterRequest request = new()
            {
                RegNumber = student.RegNumber!,
                ElectionId = election.Id
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Voter registration has ended for this election.", result.Error);

            factory.RegisteredVoterRepository.Verify(x => x.AddAsync(It.IsAny<RegisteredVoter>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RegisterVoter_ReturnsForbidden_WhenStudentDoesNotBelongToElectionDepartment()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory);
            election.DepartmentId = student.DepartmentId + 1;

            factory.DbContextFactory.Context.Elections.Update(election);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            RegisterVoterRequest request = new()
            {
                RegNumber = student.RegNumber!,
                ElectionId = election.Id
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Student is not eligible to register for this election.", result.Error);

            factory.RegisteredVoterRepository.Verify(x => x.AddAsync(It.IsAny<RegisteredVoter>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RegisterVoter_ReturnsForbidden_WhenStudentDoesNotBelongToElectionFaculty()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory);
            election.FacultyId = student.Department!.FacultyId + 1;

            factory.DbContextFactory.Context.Elections.Update(election);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            RegisterVoterRequest request = new()
            {
                RegNumber = student.RegNumber!,
                ElectionId = election.Id
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Student is not eligible to register for this election.", result.Error);

            factory.RegisteredVoterRepository.Verify(x => x.AddAsync(It.IsAny<RegisteredVoter>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RegisterVoter_ReturnsConflict_WhenStudentIsAlreadyRegistered()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory);

            RegisteredVoter registeredVoter = new()
            {
                StudentId = student.Id,
                ElectionId = election.Id,
                VotingCode = Guid.NewGuid().ToString("N")
            };

            factory.DbContextFactory.Context.RegisteredVoters.Add(registeredVoter);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            RegisterVoterRequest request = new()
            {
                RegNumber = student.RegNumber!,
                ElectionId = election.Id
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Student is already registered for this election.", result.Error);

            factory.RegisteredVoterRepository.Verify(x => x.AddAsync(It.IsAny<RegisteredVoter>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task RegisterVoter_ReturnsCreated_WhenEmailQueueFails()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory);

            SetupRegisteredVoterMapping(factory, student, election);

            factory.BackgroundTaskQueue
                .Setup(x => x.Enqueue<SendVoterEmailTask, VoterEmailDto>(It.IsAny<VoterEmailDto>()))
                .Throws(new InvalidOperationException("Queue unavailable"));

            RegisterVoterRequest request = new()
            {
                RegNumber = student.RegNumber!,
                ElectionId = election.Id
            };

            Result<RegisteredVoterResponse> result = await factory.Service.RegisterVoter(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.NotNull(result.Value);

            RegisteredVoter? registeredVoter = factory.DbContextFactory.Context.RegisteredVoters
                .FirstOrDefault(x => x.StudentId == student.Id && x.ElectionId == election.Id);

            Assert.NotNull(registeredVoter);

            factory.LoggerMessage.Verify(x => x.LogError(It.Is<string>(message =>
                message.Contains($"Voter email could not be queued for student {student.Id}."))), Times.Once);

            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.RegisteredVoter, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRegisteredVoter_ReturnsSuccess_WhenRegisteredVoterExists()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory);

            RegisteredVoter registeredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionId = election.Id,
                VotingCode = "VOTE123",
                Active = true
            };

            factory.DbContextFactory.Context.RegisteredVoters.Add(registeredVoter);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupRegisteredVoterMapping(factory, student, election);

            Result<RegisteredVoterResponse> result = await factory.Service.GetRegisteredVoter(registeredVoter.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(registeredVoter.Id, result.Value.RegisteredVoterId);
            Assert.Equal(student.Id, result.Value.StudentId);
            Assert.Equal(student.RegNumber, result.Value.RegistrationNumber);
            Assert.Equal($"{student.User!.FirstName} {student.User.LastName}", result.Value.StudentName);
            Assert.Equal(election.Id, result.Value.ElectionId);
            Assert.Equal(election.Name, result.Value.ElectionName);
            Assert.Equal(registeredVoter.VotingCode, result.Value.VotingCode);
            Assert.True(result.Value.Active);
        }

        [Fact]
        public async Task GetRegisteredVoter_ReturnsNotFound_WhenRegisteredVoterDoesNotExist()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            string registeredVoterId = Guid.NewGuid().ToString();

            Result<RegisteredVoterResponse> result = await factory.Service.GetRegisteredVoter(registeredVoterId);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal($"Registered voter with id {registeredVoterId} was not found.", result.Error);
        }

        [Fact]
        public async Task GetRegisteredVoters_ReturnsOnlyVotersForSpecifiedElection()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student firstStudent = await CreateStudent(factory);

            User secondUser = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                UserName = "jane.doe@example.com",
                Active = true
            };

            Student secondStudent = new()
            {
                Id = Guid.NewGuid(),
                RegNumber = $"REG-{Guid.NewGuid():N}",
                UserId = secondUser.Id,
                User = secondUser,
                DepartmentId = firstStudent.DepartmentId,
                Department = firstStudent.Department,
                GenderId = 1,
                Active = true
            };

            factory.DbContextFactory.Context.Users.Add(secondUser);
            factory.DbContextFactory.Context.Students.Add(secondStudent);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            Election firstElection = await CreateElection(factory, name: "Engineering Election 2026", electionTypeId: 1, yearId: 1);

            Election secondElection = await CreateElection(factory, name: "General Election 2026", electionTypeId: 2, yearId: 1);

            RegisteredVoter firstRegisteredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = firstStudent.Id,
                ElectionId = firstElection.Id,
                VotingCode = "VOTE001",
                Active = true
            };

            RegisteredVoter secondRegisteredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = secondStudent.Id,
                ElectionId = secondElection.Id,
                VotingCode = "VOTE002",
                Active = true
            };

            factory.DbContextFactory.Context.RegisteredVoters.AddRange(firstRegisteredVoter, secondRegisteredVoter);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(x => x.Map<PagedResponse<RegisteredVoterResponse>>(It.IsAny<PagedList<RegisteredVoter>>()))
                .Returns((PagedList<RegisteredVoter> registeredVoters) => new PagedResponse<RegisteredVoterResponse>
                {
                    Items = registeredVoters.Select(registeredVoter => new RegisteredVoterResponse
                    {
                        RegisteredVoterId = registeredVoter.Id,
                        StudentId = registeredVoter.StudentId,
                        RegistrationNumber = registeredVoter.Student.RegNumber ?? string.Empty,
                        StudentName = $"{registeredVoter.Student.User!.FirstName} {registeredVoter.Student.User.LastName}",
                        ElectionId = registeredVoter.ElectionId,
                        ElectionName = registeredVoter.Election.Name,
                        VotingCode = registeredVoter.VotingCode,
                        Active = registeredVoter.Active
                    }).ToList(),
                    MetaData = registeredVoters.MetaData
                });

            RegisteredVoterRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = firstElection.Id
            };

            Result<PagedResponse<RegisteredVoterResponse>> result = await factory.Service.GetRegisteredVoters(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            RegisteredVoterResponse registeredVoterResponse = result.Value.Items.Single();

            Assert.Equal(firstRegisteredVoter.Id, registeredVoterResponse.RegisteredVoterId);
            Assert.Equal(firstStudent.Id, registeredVoterResponse.StudentId);
            Assert.Equal(firstElection.Id, registeredVoterResponse.ElectionId);
            Assert.Equal(firstElection.Name, registeredVoterResponse.ElectionName);
        }

        [Fact]
        public async Task GetRegisteredVoters_ReturnsOnlyVotersForSpecifiedStudent()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student firstStudent = await CreateStudent(factory);

            User secondUser = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                UserName = "jane.doe@example.com",
                Active = true
            };

            Student secondStudent = new()
            {
                Id = Guid.NewGuid(),
                RegNumber = $"REG-{Guid.NewGuid():N}",
                UserId = secondUser.Id,
                User = secondUser,
                DepartmentId = firstStudent.DepartmentId,
                Department = firstStudent.Department,
                GenderId = 1,
                Active = true
            };

            factory.DbContextFactory.Context.Users.Add(secondUser);
            factory.DbContextFactory.Context.Students.Add(secondStudent);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            Election election = await CreateElection(factory);

            RegisteredVoter firstRegisteredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = firstStudent.Id,
                ElectionId = election.Id,
                VotingCode = "VOTE001",
                Active = true
            };

            RegisteredVoter secondRegisteredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = secondStudent.Id,
                ElectionId = election.Id,
                VotingCode = "VOTE002",
                Active = true
            };

            factory.DbContextFactory.Context.RegisteredVoters.AddRange(firstRegisteredVoter, secondRegisteredVoter);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupRegisteredVotersPagedMapping(factory);

            RegisteredVoterRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                StudentId = firstStudent.Id
            };

            Result<PagedResponse<RegisteredVoterResponse>> result = await factory.Service.GetRegisteredVoters(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            RegisteredVoterResponse registeredVoterResponse = result.Value.Items.Single();

            Assert.Equal(firstRegisteredVoter.Id, registeredVoterResponse.RegisteredVoterId);
            Assert.Equal(firstStudent.Id, registeredVoterResponse.StudentId);
        }

        [Fact]
        public async Task GetRegisteredVoters_ReturnsOnlyVotersForSpecifiedActiveStatus()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student firstStudent = await CreateStudent(factory);

            User secondUser = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                UserName = "jane.doe@example.com",
                Active = true
            };

            Student secondStudent = new()
            {
                Id = Guid.NewGuid(),
                RegNumber = $"REG-{Guid.NewGuid():N}",
                UserId = secondUser.Id,
                User = secondUser,
                DepartmentId = firstStudent.DepartmentId,
                Department = firstStudent.Department,
                GenderId = 1,
                Active = true
            };

            factory.DbContextFactory.Context.Users.Add(secondUser);
            factory.DbContextFactory.Context.Students.Add(secondStudent);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            Election election = await CreateElection(factory);

            RegisteredVoter activeRegisteredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = firstStudent.Id,
                ElectionId = election.Id,
                VotingCode = "VOTE001",
                Active = true
            };

            RegisteredVoter inactiveRegisteredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = secondStudent.Id,
                ElectionId = election.Id,
                VotingCode = "VOTE002",
                Active = false
            };

            factory.DbContextFactory.Context.RegisteredVoters.AddRange(activeRegisteredVoter, inactiveRegisteredVoter);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupRegisteredVotersPagedMapping(factory);

            RegisteredVoterRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                Active = true
            };

            Result<PagedResponse<RegisteredVoterResponse>> result = await factory.Service.GetRegisteredVoters(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            RegisteredVoterResponse registeredVoterResponse = result.Value.Items.Single();

            Assert.Equal(activeRegisteredVoter.Id, registeredVoterResponse.RegisteredVoterId);
            Assert.True(registeredVoterResponse.Active);
        }

        [Theory]
        [InlineData("RegistrationNumber")]
        [InlineData("FirstName")]
        [InlineData("LastName")]
        [InlineData("ElectionName")]
        public async Task GetRegisteredVoters_ReturnsMatchingVoter_WhenSearchTermMatchesSupportedField(string searchField)
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student student = await CreateStudent(factory);
            Election election = await CreateElection(factory, name: "Engineering Student Election");

            RegisteredVoter registeredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionId = election.Id,
                VotingCode = "VOTE001",
                Active = true
            };

            factory.DbContextFactory.Context.RegisteredVoters.Add(registeredVoter);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            SetupRegisteredVotersPagedMapping(factory);

            string searchTerm = searchField switch
            {
                "RegistrationNumber" => student.RegNumber!,
                "FirstName" => student.User!.FirstName!,
                "LastName" => student.User!.LastName!,
                "ElectionName" => election.Name,
                _ => string.Empty
            };

            RegisteredVoterRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = searchTerm
            };

            Result<PagedResponse<RegisteredVoterResponse>> result = await factory.Service.GetRegisteredVoters(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            RegisteredVoterResponse registeredVoterResponse = result.Value.Items.Single();

            Assert.Equal(registeredVoter.Id, registeredVoterResponse.RegisteredVoterId);
        }

        [Fact]
        public async Task GetRegisteredVoters_ReturnsRequestedPage()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student firstStudent = await CreateStudent(factory);

            User secondUser = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                UserName = "jane.doe@example.com",
                Active = true
            };

            Student secondStudent = new()
            {
                Id = Guid.NewGuid(),
                RegNumber = $"REG-{Guid.NewGuid():N}",
                UserId = secondUser.Id,
                User = secondUser,
                DepartmentId = firstStudent.DepartmentId,
                Department = firstStudent.Department,
                GenderId = 1,
                Active = true
            };

            factory.DbContextFactory.Context.Users.Add(secondUser);
            factory.DbContextFactory.Context.Students.Add(secondStudent);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            Election election = await CreateElection(factory);

            RegisteredVoter firstRegisteredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = firstStudent.Id,
                ElectionId = election.Id,
                VotingCode = "VOTE001",
                Active = true,
            };

            RegisteredVoter secondRegisteredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = secondStudent.Id,
                ElectionId = election.Id,
                VotingCode = "VOTE002",
                Active = true,
            };

            factory.DbContextFactory.Context.RegisteredVoters.AddRange(firstRegisteredVoter, secondRegisteredVoter);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            await factory.DbContextFactory.Context.RegisteredVoters.Where(x => x.Id == firstRegisteredVoter.Id)
                .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.CreatedAt, DateTime.UtcNow.AddMinutes(-2)));

            await factory.DbContextFactory.Context.RegisteredVoters.Where(x => x.Id == secondRegisteredVoter.Id)
                .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.CreatedAt, DateTime.UtcNow.AddMinutes(-1)));

            SetupRegisteredVotersPagedMapping(factory);

            RegisteredVoterRequest request = new()
            {
                PageNumber = 2,
                PageSize = 1
            };

            Result<PagedResponse<RegisteredVoterResponse>> result = await factory.Service.GetRegisteredVoters(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            RegisteredVoterResponse registeredVoterResponse = result.Value.Items.Single();

            Assert.Equal(firstRegisteredVoter.Id, registeredVoterResponse.RegisteredVoterId);
        }

        [Fact]
        public async Task GetRegisteredVoters_ReturnsCachedResponse_WhenCacheHitOccurs()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            RegisteredVoterRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                Active = true
            };

            RegisteredVoterResponse registeredVoter = new()
            {
                RegisteredVoterId = Guid.NewGuid().ToString(),
                StudentId = Guid.NewGuid(),
                RegistrationNumber = "REG001",
                StudentName = "John Doe",
                ElectionId = Guid.NewGuid().ToString(),
                ElectionName = "Student Election 2026",
                VotingCode = "VOTE001",
                Active = true
            };

            PagedResponse<RegisteredVoterResponse> cachedResponse = new()
            {
                Items = new List<RegisteredVoterResponse>
                {
                    registeredVoter
                }
            };

            factory.CacheService.Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<RegisteredVoterResponse>>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedResponse);

            Result<PagedResponse<RegisteredVoterResponse>> result = await factory.Service.GetRegisteredVoters(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Same(cachedResponse, result.Value);

            factory.RegisteredVoterRepository.Verify(x => x.GetQueryable(
                It.IsAny<Expression<Func<RegisteredVoter, bool>>?>(),
                It.IsAny<Func<IQueryable<RegisteredVoter>, IOrderedQueryable<RegisteredVoter>>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<RegisteredVoter>, IIncludableQueryable<RegisteredVoter, object>>?>()), Times.Never);
        }

        [Fact]
        public async Task GetRegisteredVoter_ReturnsCachedResponse_WhenCacheHitOccurs()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            string registeredVoterId = Guid.NewGuid().ToString();

            RegisteredVoterResponse cachedResponse = new()
            {
                RegisteredVoterId = registeredVoterId,
                StudentId = Guid.NewGuid(),
                RegistrationNumber = "REG001",
                StudentName = "John Doe",
                ElectionId = Guid.NewGuid().ToString(),
                ElectionName = "Student Election 2026",
                VotingCode = "VOTE001",
                Active = true
            };

            factory.CacheService.Setup(x => x.GetOrCreate(It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, ValueTask<RegisteredVoterResponse?>>>(),
                    It.IsAny<CacheEntryOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<RegisteredVoterResponse?>(cachedResponse));

            Result<RegisteredVoterResponse> result = await factory.Service.GetRegisteredVoter(registeredVoterId);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Same(cachedResponse, result.Value);

            factory.RegisteredVoterRepository.Verify(x => x.GetSingleByAsync(
                It.IsAny<Expression<Func<RegisteredVoter, bool>>?>(),
                It.IsAny<Func<IQueryable<RegisteredVoter>, IOrderedQueryable<RegisteredVoter>>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<RegisteredVoter>, IIncludableQueryable<RegisteredVoter, object>>?>(),
                It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsCreated_WhenVoteIsSuccessful()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);

            User contestantUser = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = $"jane.doe.{Guid.NewGuid():N}@example.com",
                UserName = $"jane.doe.{Guid.NewGuid():N}@example.com",
                Active = true
            };

            Student contestantStudent = new()
            {
                Id = Guid.NewGuid(),
                RegNumber = $"REG-{Guid.NewGuid():N}",
                UserId = contestantUser.Id,
                User = contestantUser,
                DepartmentId = voter.DepartmentId,
                Department = voter.Department,
                GenderId = 1,
                Active = true
            };

            factory.DbContextFactory.Context.Users.Add(contestantUser);
            factory.DbContextFactory.Context.Students.Add(contestantStudent);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            Election election = await CreateVotingElection(factory);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);
            Contestant contestant = await CreateContestant(factory, contestantStudent, electionPosition);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = contestant.Id
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("Vote cast successfully.", result.Value);

            Vote? vote = factory.DbContextFactory.Context.Votes.FirstOrDefault(x => x.RegisteredVoterId == registeredVoter.Id && x.ElectionPositionId == electionPosition.Id);

            Assert.NotNull(vote);
            Assert.Equal(contestant.Id, vote.ContestantId);

            factory.VoteRepository.Verify(x => x.AddAsync(It.Is<Vote>(vote => vote.RegisteredVoterId == registeredVoter.Id && vote.ElectionPositionId == electionPosition.Id
                && vote.ContestantId == contestant.Id), It.IsAny<bool>()), Times.Once);

            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.VoteHistory, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(x => x.RemoveByTag(CacheTags.ElectionResult, It.IsAny<CancellationToken>()), Times.Once);

            factory.BackgroundTaskQueue.Verify(x => x.Enqueue<SendVoteConfirmationEmailTask, VoteConfirmationEmailRequest>(
                It.Is<VoteConfirmationEmailRequest>(emailRequest => emailRequest.Email == voter.User!.Email
                    && emailRequest.FirstName == voter.User.FirstName && emailRequest.ElectionName == election.Name
                    && emailRequest.PositionName == electionPosition.Position.Name)), Times.Once);
        }

        [Fact]
        public async Task CastVote_ReturnsUnauthorized_WhenAuthenticatedUserCannotBeIdentified()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            CastVoteRequest request = new()
            {
                RegisteredVoterId = Guid.NewGuid().ToString(),
                VotingCode = "VOTE123",
                ElectionPositionId = Guid.NewGuid().ToString(),
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("User is not authenticated.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsForbidden_WhenVoterCredentialsAreInvalid()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Election election = await CreateVotingElection(factory);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = "INVALID-CODE",
                ElectionPositionId = Guid.NewGuid().ToString(),
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Invalid voter credentials.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsForbidden_WhenVoterCredentialsBelongToAnotherUser()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student anotherStudent = await CreateStudentInSameDepartment(factory, voter);
            Election election = await CreateVotingElection(factory);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(anotherStudent.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = Guid.NewGuid().ToString(),
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The voter credentials do not belong to the authenticated user.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsForbidden_WhenRegisteredVoterIsInactive()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Election election = await CreateVotingElection(factory);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election, active: false);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = Guid.NewGuid().ToString(),
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Registered voter is not permitted to vote.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsNotFound_WhenElectionPositionDoesNotExist()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Election election = await CreateVotingElection(factory);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);
            string electionPositionId = Guid.NewGuid().ToString();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPositionId,
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal($"Election position with id {electionPositionId} was not found.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsConflict_WhenElectionPositionIsInactive()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Election election = await CreateVotingElection(factory);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election, active: false);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Voting is not available for this election position.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsForbidden_WhenVoterBelongsToDifferentElection()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);

            Election voterElection = await CreateVotingElection(factory, electionTypeId: 1, yearId: 1,
                name: "Engineering Election 2026");

            Election positionElection = await CreateVotingElection(factory, electionTypeId: 2, yearId: 1,
                name: "General Election 2026");

            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, voterElection);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, positionElection);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Forbidden, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Registered voter is not eligible to vote in this election.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsConflict_WhenElectionIsInactive()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Election election = await CreateVotingElection(factory, active: false);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Voting is not available for this election.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsConflict_WhenVotingPeriodIsNotConfigured()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Election election = await CreateVotingElection(factory, configureVotingPeriod: false);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Voting period is not configured for this election.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsConflict_WhenVotingHasNotStarted()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Election election = await CreateVotingElection(factory,
                votingStartAt: DateTime.UtcNow.AddHours(1), votingEndAt: DateTime.UtcNow.AddHours(2));
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Voting has not started for this election.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsConflict_WhenVotingHasEnded()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Election election = await CreateVotingElection(factory,
                votingStartAt: DateTime.UtcNow.AddHours(-2), votingEndAt: DateTime.UtcNow.AddHours(-1));
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = Guid.NewGuid().ToString()
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("Voting has ended for this election.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsNotFound_WhenContestantDoesNotExist()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Election election = await CreateVotingElection(factory);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);
            string contestantId = Guid.NewGuid().ToString();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = contestantId
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Null(result.Value);
            Assert.Equal($"Contestant with id {contestantId} was not found.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsConflict_WhenContestantIsInactive()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student contestantStudent = await CreateStudentInSameDepartment(factory, voter);
            Election election = await CreateVotingElection(factory);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);
            Contestant contestant = await CreateContestant(factory, contestantStudent, electionPosition, active: false);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = contestant.Id
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The selected contestant is not available for voting.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsValidationError_WhenContestantBelongsToDifferentPosition()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student contestantStudent = await CreateStudentInSameDepartment(factory, voter);
            Election election = await CreateVotingElection(factory);

            ElectionPosition selectedElectionPosition = await CreateElectionPosition(factory, election,
                positionName: "President");

            ElectionPosition contestantElectionPosition = await CreateElectionPosition(factory, election,
                positionName: "Vice President");

            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);
            Contestant contestant = await CreateContestant(factory, contestantStudent, contestantElectionPosition);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = selectedElectionPosition.Id,
                ContestantId = contestant.Id
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("The selected contestant does not belong to this election position.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsConflict_WhenVoteAlreadyExists()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student contestantStudent = await CreateStudentInSameDepartment(factory, voter);
            Election election = await CreateVotingElection(factory);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);
            Contestant contestant = await CreateContestant(factory, contestantStudent, electionPosition);

            Vote existingVote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = registeredVoter.Id,
                ContestantId = contestant.Id,
                ElectionPositionId = electionPosition.Id,
                VotedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            factory.DbContextFactory.Context.Votes.Add(existingVote);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = contestant.Id
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Null(result.Value);
            Assert.Equal("A vote has already been cast for this election position.", result.Error);

            factory.VoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CastVote_ReturnsCreated_WhenEmailQueueFails()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student contestantStudent = await CreateStudentInSameDepartment(factory, voter);
            Election election = await CreateVotingElection(factory);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);
            Contestant contestant = await CreateContestant(factory, contestantStudent, electionPosition);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            factory.BackgroundTaskQueue
                .Setup(x => x.Enqueue<SendVoteConfirmationEmailTask, VoteConfirmationEmailRequest>(
                    It.IsAny<VoteConfirmationEmailRequest>()))
                .Throws(new InvalidOperationException("Queue unavailable"));

            CastVoteRequest request = new()
            {
                RegisteredVoterId = registeredVoter.Id,
                VotingCode = registeredVoter.VotingCode,
                ElectionPositionId = electionPosition.Id,
                ContestantId = contestant.Id
            };

            Result<string> result = await factory.Service.CastVote(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("Vote cast successfully.", result.Value);

            Vote? vote = factory.DbContextFactory.Context.Votes.FirstOrDefault(x =>
                x.RegisteredVoterId == registeredVoter.Id &&
                x.ElectionPositionId == electionPosition.Id);

            Assert.NotNull(vote);
            Assert.Equal(contestant.Id, vote.ContestantId);

            factory.LoggerMessage.Verify(x => x.LogError(It.Is<string>(message =>
                message.Contains($"Vote confirmation email could not be queued for registered voter {registeredVoter.Id}."))), Times.Once);
        }

        [Fact]
        public async Task GetMyVotes_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns((string?)null);

            VoteHistoryRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<VoteHistoryResponse>> result = await factory.Service.GetMyVotes(request);

            Assert.Equal(ResultStatus.Unauthorized, result.Status);
            Assert.Equal("User is not authenticated.", result.Error);

            factory.VoteRepository.Verify(x => x.GetQueryable(It.IsAny<Expression<Func<Vote, bool>>?>(),
                It.IsAny<Func<IQueryable<Vote>, IOrderedQueryable<Vote>>?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Vote>, IIncludableQueryable<Vote, object>>?>()), Times.Never);
        }

        [Fact]
        public async Task GetMyVotes_ReturnsVotesBelongingToAuthenticatedUser()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student otherVoter = await CreateStudentInSameDepartment(factory, voter, "Peter", "Brown");
            Student contestantStudent = await CreateStudentInSameDepartment(factory, voter, "Jane", "Doe");

            Election election = await CreateVotingElection(factory);
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            Contestant contestant = await CreateContestant(factory, contestantStudent, electionPosition);

            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);
            RegisteredVoter otherRegisteredVoter = await CreateRegisteredVoter(factory, otherVoter, election);

            Vote vote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = registeredVoter.Id,
                ContestantId = contestant.Id,
                ElectionPositionId = electionPosition.Id,
                VotedAt = DateTime.UtcNow
            };

            Vote otherVote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = otherRegisteredVoter.Id,
                ContestantId = contestant.Id,
                ElectionPositionId = electionPosition.Id,
                VotedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.Votes.AddRange(vote, otherVote);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            SetupVoteHistoryPagedMapping(factory);

            VoteHistoryRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<VoteHistoryResponse>> result = await factory.Service.GetMyVotes(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Single(result.Value.Items);

            VoteHistoryResponse voteHistory = result.Value.Items.Single();

            Assert.Equal(vote.Id, voteHistory.VoteId);
            Assert.Equal(election.Id, voteHistory.ElectionId);
            Assert.Equal(election.Name, voteHistory.ElectionName);
            Assert.Equal(electionPosition.Id, voteHistory.ElectionPositionId);
            Assert.Equal(electionPosition.Position.Name, voteHistory.PositionName);
            Assert.Equal(contestant.Id, voteHistory.ContestantId);
            Assert.Equal($"{contestantStudent.User!.FirstName} {contestantStudent.User.LastName}", voteHistory.ContestantName);
        }

        [Fact]
        public async Task GetMyVotes_ReturnsOnlyVotesForSpecifiedElection()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student contestantStudent = await CreateStudentInSameDepartment(factory, voter);

            Election firstElection = await CreateVotingElection(factory, electionTypeId: 1, name: "Engineering Election");
            Election secondElection = await CreateVotingElection(factory, electionTypeId: 2, name: "General Election");

            ElectionPosition firstElectionPosition = await CreateElectionPosition(factory, firstElection, positionName: "President");
            ElectionPosition secondElectionPosition = await CreateElectionPosition(factory, secondElection, positionName: "Secretary");

            Contestant firstContestant = await CreateContestant(factory, contestantStudent, firstElectionPosition);
            Contestant secondContestant = await CreateContestant(factory, contestantStudent, secondElectionPosition);

            RegisteredVoter firstRegisteredVoter = await CreateRegisteredVoter(factory, voter, firstElection);
            RegisteredVoter secondRegisteredVoter = await CreateRegisteredVoter(factory, voter, secondElection);

            Vote firstVote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = firstRegisteredVoter.Id,
                ContestantId = firstContestant.Id,
                ElectionPositionId = firstElectionPosition.Id,
                VotedAt = DateTime.UtcNow
            };

            Vote secondVote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = secondRegisteredVoter.Id,
                ContestantId = secondContestant.Id,
                ElectionPositionId = secondElectionPosition.Id,
                VotedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.Votes.AddRange(firstVote, secondVote);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            SetupVoteHistoryPagedMapping(factory);

            VoteHistoryRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = firstElection.Id
            };

            Result<PagedResponse<VoteHistoryResponse>> result = await factory.Service.GetMyVotes(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            VoteHistoryResponse voteHistory = result.Value.Items.Single();

            Assert.Equal(firstVote.Id, voteHistory.VoteId);
            Assert.Equal(firstElection.Id, voteHistory.ElectionId);
        }

        [Fact]
        public async Task GetMyVotes_UsesVoteHistoryCachePolicy()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            SetupVoteHistoryPagedMapping(factory);

            VoteHistoryRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<VoteHistoryResponse>> result = await factory.Service.GetMyVotes(request);

            Assert.Equal(ResultStatus.Success, result.Status);

            factory.CacheService.Verify(x => x.GetOrCreate(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<VoteHistoryResponse>>>>(),
                CachePolicies.VoteHistory,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMyVotes_ReturnsOnlyVotesForSpecifiedElectionPosition()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student firstContestantStudent = await CreateStudentInSameDepartment(factory, voter, "Jane", "Doe");
            Student secondContestantStudent = await CreateStudentInSameDepartment(factory, voter, "John", "Smith");

            Election election = await CreateVotingElection(factory);
            ElectionPosition firstElectionPosition = await CreateElectionPosition(factory, election, positionName: "President");
            ElectionPosition secondElectionPosition = await CreateElectionPosition(factory, election, positionName: "Secretary");

            Contestant firstContestant = await CreateContestant(factory, firstContestantStudent, firstElectionPosition);
            Contestant secondContestant = await CreateContestant(factory, secondContestantStudent, secondElectionPosition);

            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            Vote firstVote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = registeredVoter.Id,
                ContestantId = firstContestant.Id,
                ElectionPositionId = firstElectionPosition.Id,
                VotedAt = DateTime.UtcNow
            };

            Vote secondVote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = registeredVoter.Id,
                ContestantId = secondContestant.Id,
                ElectionPositionId = secondElectionPosition.Id,
                VotedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.Votes.AddRange(firstVote, secondVote);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            SetupVoteHistoryPagedMapping(factory);

            VoteHistoryRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionPositionId = firstElectionPosition.Id
            };

            Result<PagedResponse<VoteHistoryResponse>> result = await factory.Service.GetMyVotes(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            VoteHistoryResponse voteHistory = result.Value.Items.Single();

            Assert.Equal(firstVote.Id, voteHistory.VoteId);
            Assert.Equal(firstElectionPosition.Id, voteHistory.ElectionPositionId);
            Assert.Equal(firstContestant.Id, voteHistory.ContestantId);
        }

        [Theory]
        [InlineData("Student Election")]
        [InlineData("President")]
        [InlineData("Jane")]
        [InlineData("Doe")]
        public async Task GetMyVotes_ReturnsVotesMatchingSearchTerm(string searchTerm)
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student contestantStudent = await CreateStudentInSameDepartment(factory, voter, "Jane", "Doe");

            Election election = await CreateVotingElection(factory, name: "Student Election 2026");
            ElectionPosition electionPosition = await CreateElectionPosition(factory, election, positionName: "President");
            Contestant contestant = await CreateContestant(factory, contestantStudent, electionPosition);
            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            Vote vote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = registeredVoter.Id,
                ContestantId = contestant.Id,
                ElectionPositionId = electionPosition.Id,
                VotedAt = DateTime.UtcNow
            };

            factory.DbContextFactory.Context.Votes.Add(vote);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            SetupVoteHistoryPagedMapping(factory);

            VoteHistoryRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = searchTerm
            };

            Result<PagedResponse<VoteHistoryResponse>> result = await factory.Service.GetMyVotes(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            VoteHistoryResponse voteHistory = result.Value.Items.Single();

            Assert.Equal(vote.Id, voteHistory.VoteId);
        }

        [Fact]
        public async Task GetMyVotes_ReturnsRequestedPage()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student voter = await CreateStudent(factory);
            Student firstContestantStudent = await CreateStudentInSameDepartment(factory, voter, "Jane", "Doe");
            Student secondContestantStudent = await CreateStudentInSameDepartment(factory, voter, "John", "Smith");

            Election election = await CreateVotingElection(factory);
            ElectionPosition firstElectionPosition = await CreateElectionPosition(factory, election, positionName: "President");
            ElectionPosition secondElectionPosition = await CreateElectionPosition(factory, election, positionName: "Secretary");

            Contestant firstContestant = await CreateContestant(factory, firstContestantStudent, firstElectionPosition);
            Contestant secondContestant = await CreateContestant(factory, secondContestantStudent, secondElectionPosition);

            RegisteredVoter registeredVoter = await CreateRegisteredVoter(factory, voter, election);

            Vote firstVote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = registeredVoter.Id,
                ContestantId = firstContestant.Id,
                ElectionPositionId = firstElectionPosition.Id,
                VotedAt = DateTime.UtcNow
            };

            Vote secondVote = new()
            {
                Id = Guid.NewGuid().ToString(),
                RegisteredVoterId = registeredVoter.Id,
                ContestantId = secondContestant.Id,
                ElectionPositionId = secondElectionPosition.Id,
                VotedAt = DateTime.UtcNow.AddMinutes(-1)
            };

            factory.DbContextFactory.Context.Votes.AddRange(firstVote, secondVote);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.CurrentUserContext.Setup(x => x.UserId).Returns(voter.UserId);

            SetupVoteHistoryPagedMapping(factory);

            VoteHistoryRequest request = new()
            {
                PageNumber = 2,
                PageSize = 1
            };

            Result<PagedResponse<VoteHistoryResponse>> result = await factory.Service.GetMyVotes(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            VoteHistoryResponse voteHistory = result.Value.Items.Single();

            Assert.Equal(secondVote.Id, voteHistory.VoteId);
        }

        [Fact]
        public async Task GetElectionResults_DoesNotReturnResultsForOngoingElection()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student contestantStudent = await CreateStudent(factory);

            Election election = await CreateVotingElection(factory, votingStartAt: DateTime.UtcNow.AddHours(-1),
                votingEndAt: DateTime.UtcNow.AddHours(1));

            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            await CreateContestant(factory, contestantStudent, electionPosition);

            ElectionResultRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = election.Id
            };

            Result<PagedResponse<ElectionResultResponse>> result = await factory.Service.GetElectionResults(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.Items);
        }

        [Fact]
        public async Task GetElectionResults_ReturnsContestantWithZeroVotes()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student contestantStudent = await CreateStudent(factory);

            Election election = await CreateVotingElection(factory, votingStartAt: DateTime.UtcNow.AddHours(-2),
                votingEndAt: DateTime.UtcNow.AddHours(-1));

            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);
            Contestant contestant = await CreateContestant(factory, contestantStudent, electionPosition);

            ElectionResultRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = election.Id
            };

            Result<PagedResponse<ElectionResultResponse>> result = await factory.Service.GetElectionResults(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            ElectionResultResponse electionResult = result.Value.Items.Single();

            Assert.Equal(contestant.Id, electionResult.ContestantId);
            Assert.Equal(0, electionResult.VoteCount);
            Assert.Equal(0, electionResult.TotalVotes);
            Assert.Equal(0, electionResult.Percentage);
        }

        [Fact]
        public async Task GetElectionResults_ReturnsCorrectVoteCountsAndPercentages()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student firstContestantStudent = await CreateStudent(factory);
            Student secondContestantStudent = await CreateStudentInSameDepartment(factory, firstContestantStudent, "John", "Smith");
            Student thirdContestantStudent = await CreateStudentInSameDepartment(factory, firstContestantStudent, "Peter", "Brown");

            Election election = await CreateVotingElection(factory, votingStartAt: DateTime.UtcNow.AddHours(-2),
                votingEndAt: DateTime.UtcNow.AddHours(-1));

            ElectionPosition electionPosition = await CreateElectionPosition(factory, election);

            Contestant firstContestant = await CreateContestant(factory, firstContestantStudent, electionPosition);
            Contestant secondContestant = await CreateContestant(factory, secondContestantStudent, electionPosition);
            Contestant thirdContestant = await CreateContestant(factory, thirdContestantStudent, electionPosition);

            Student firstVoter = await CreateStudentInSameDepartment(factory, firstContestantStudent, "Voter", "One");
            Student secondVoter = await CreateStudentInSameDepartment(factory, firstContestantStudent, "Voter", "Two");
            Student thirdVoter = await CreateStudentInSameDepartment(factory, firstContestantStudent, "Voter", "Three");
            Student fourthVoter = await CreateStudentInSameDepartment(factory, firstContestantStudent, "Voter", "Four");
            Student fifthVoter = await CreateStudentInSameDepartment(factory, firstContestantStudent, "Voter", "Five");

            RegisteredVoter firstRegisteredVoter = await CreateRegisteredVoter(factory, firstVoter, election);
            RegisteredVoter secondRegisteredVoter = await CreateRegisteredVoter(factory, secondVoter, election);
            RegisteredVoter thirdRegisteredVoter = await CreateRegisteredVoter(factory, thirdVoter, election);
            RegisteredVoter fourthRegisteredVoter = await CreateRegisteredVoter(factory, fourthVoter, election);
            RegisteredVoter fifthRegisteredVoter = await CreateRegisteredVoter(factory, fifthVoter, election);

            List<Vote> votes =
            [
                new Vote
        {
            RegisteredVoterId = firstRegisteredVoter.Id,
            ContestantId = firstContestant.Id,
            ElectionPositionId = electionPosition.Id,
            VotedAt = DateTime.UtcNow.AddHours(-1)
        },
        new Vote
        {
            RegisteredVoterId = secondRegisteredVoter.Id,
            ContestantId = firstContestant.Id,
            ElectionPositionId = electionPosition.Id,
            VotedAt = DateTime.UtcNow.AddHours(-1)
        },
        new Vote
        {
            RegisteredVoterId = thirdRegisteredVoter.Id,
            ContestantId = firstContestant.Id,
            ElectionPositionId = electionPosition.Id,
            VotedAt = DateTime.UtcNow.AddHours(-1)
        },
        new Vote
        {
            RegisteredVoterId = fourthRegisteredVoter.Id,
            ContestantId = secondContestant.Id,
            ElectionPositionId = electionPosition.Id,
            VotedAt = DateTime.UtcNow.AddHours(-1)
        },
        new Vote
        {
            RegisteredVoterId = fifthRegisteredVoter.Id,
            ContestantId = secondContestant.Id,
            ElectionPositionId = electionPosition.Id,
            VotedAt = DateTime.UtcNow.AddHours(-1)
        }
            ];

            factory.DbContextFactory.Context.Votes.AddRange(votes);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionResultRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = election.Id
            };

            Result<PagedResponse<ElectionResultResponse>> result = await factory.Service.GetElectionResults(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value.Items.Count());

            ElectionResultResponse firstResult = result.Value.Items.Single(x => x.ContestantId == firstContestant.Id);
            ElectionResultResponse secondResult = result.Value.Items.Single(x => x.ContestantId == secondContestant.Id);
            ElectionResultResponse thirdResult = result.Value.Items.Single(x => x.ContestantId == thirdContestant.Id);

            Assert.Equal(3, firstResult.VoteCount);
            Assert.Equal(5, firstResult.TotalVotes);
            Assert.Equal(60m, firstResult.Percentage);

            Assert.Equal(2, secondResult.VoteCount);
            Assert.Equal(5, secondResult.TotalVotes);
            Assert.Equal(40m, secondResult.Percentage);

            Assert.Equal(0, thirdResult.VoteCount);
            Assert.Equal(5, thirdResult.TotalVotes);
            Assert.Equal(0m, thirdResult.Percentage);
        }

        [Fact]
        public async Task GetElectionResults_ReturnsOnlyResultsForSpecifiedElectionPosition()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student firstContestantStudent = await CreateStudent(factory);
            Student secondContestantStudent = await CreateStudentInSameDepartment(factory, firstContestantStudent, "John", "Smith");

            Election election = await CreateVotingElection(factory, votingStartAt: DateTime.UtcNow.AddHours(-2),
                votingEndAt: DateTime.UtcNow.AddHours(-1));

            ElectionPosition presidentPosition = await CreateElectionPosition(factory, election, positionName: "President");
            ElectionPosition secretaryPosition = await CreateElectionPosition(factory, election, positionName: "Secretary");

            Contestant presidentContestant = await CreateContestant(factory, firstContestantStudent, presidentPosition);
            await CreateContestant(factory, secondContestantStudent, secretaryPosition);

            ElectionResultRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionPositionId = presidentPosition.Id
            };

            Result<PagedResponse<ElectionResultResponse>> result = await factory.Service.GetElectionResults(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            ElectionResultResponse electionResult = result.Value.Items.Single();

            Assert.Equal(presidentPosition.Id, electionResult.ElectionPositionId);
            Assert.Equal(presidentContestant.Id, electionResult.ContestantId);
            Assert.Equal("President", electionResult.PositionName);
        }

        [Fact]
        public async Task GetElectionResults_UsesElectionResultCachePolicy()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            ElectionResultRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionResultResponse>> result = await factory.Service.GetElectionResults(request);

            Assert.Equal(ResultStatus.Success, result.Status);

            factory.CacheService.Verify(x => x.GetOrCreate(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<ElectionResultResponse>>>>(),
                CachePolicies.ElectionResult,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetElectionResults_ReturnsOnlyResultsForSpecifiedElection()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student firstContestantStudent = await CreateStudent(factory);
            Student secondContestantStudent = await CreateStudentInSameDepartment(factory, firstContestantStudent, "John", "Smith");

            Election firstElection = await CreateVotingElection(factory, votingStartAt: DateTime.UtcNow.AddHours(-3),
                votingEndAt: DateTime.UtcNow.AddHours(-2), electionTypeId: 1, name: "Engineering Election 2026");

            Election secondElection = await CreateVotingElection(factory, votingStartAt: DateTime.UtcNow.AddHours(-3),
                votingEndAt: DateTime.UtcNow.AddHours(-2), electionTypeId: 2, name: "General Election 2026");

            ElectionPosition firstElectionPosition = await CreateElectionPosition(factory, firstElection, positionName: "President");
            ElectionPosition secondElectionPosition = await CreateElectionPosition(factory, secondElection, positionName: "Secretary");

            Contestant firstContestant = await CreateContestant(factory, firstContestantStudent, firstElectionPosition);
            await CreateContestant(factory, secondContestantStudent, secondElectionPosition);

            ElectionResultRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = firstElection.Id
            };

            Result<PagedResponse<ElectionResultResponse>> result = await factory.Service.GetElectionResults(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            ElectionResultResponse electionResult = result.Value.Items.Single();

            Assert.Equal(firstElection.Id, electionResult.ElectionId);
            Assert.Equal(firstContestant.Id, electionResult.ContestantId);
        }

        [Theory]
        [InlineData("Engineering Election")]
        [InlineData("President")]
        [InlineData("Jane")]
        [InlineData("Doe")]
        public async Task GetElectionResults_ReturnsResultsMatchingSearchTerm(string searchTerm)
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student baseStudent = await CreateStudent(factory);
            Student contestantStudent = await CreateStudentInSameDepartment(factory, baseStudent, "Jane", "Doe");

            Election election = await CreateVotingElection(factory, votingStartAt: DateTime.UtcNow.AddHours(-3),
                votingEndAt: DateTime.UtcNow.AddHours(-2), name: "Engineering Election 2026");

            ElectionPosition electionPosition = await CreateElectionPosition(factory, election, positionName: "President");
            Contestant contestant = await CreateContestant(factory, contestantStudent, electionPosition);

            ElectionResultRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = searchTerm
            };

            Result<PagedResponse<ElectionResultResponse>> result = await factory.Service.GetElectionResults(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            ElectionResultResponse electionResult = result.Value.Items.Single();

            Assert.Equal(contestant.Id, electionResult.ContestantId);
        }

        
        [Fact]
        public async Task GetElectionResults_ReturnsRequestedPage()
        {
            using VoterServiceFactory factory = new VoterServiceFactory();

            Student firstContestantStudent = await CreateStudent(factory);
            Student secondContestantStudent = await CreateStudentInSameDepartment(factory, firstContestantStudent, "John", "Smith");

            Election election = await CreateVotingElection(factory, votingStartAt: DateTime.UtcNow.AddHours(-3),
                votingEndAt: DateTime.UtcNow.AddHours(-2));

            ElectionPosition electionPosition = await CreateElectionPosition(factory, election, positionName: "President");

            Contestant firstContestant = await CreateContestant(factory, firstContestantStudent, electionPosition);
            Contestant secondContestant = await CreateContestant(factory, secondContestantStudent, electionPosition);

            ElectionResultRequest request = new()
            {
                PageNumber = 2,
                PageSize = 1
            };

            Result<PagedResponse<ElectionResultResponse>> result = await factory.Service.GetElectionResults(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);

            ElectionResultResponse electionResult = result.Value.Items.Single();

            Assert.Contains(electionResult.ContestantId, new[] { firstContestant.Id, secondContestant.Id });
            Assert.Equal(2, result.Value.MetaData.TotalCount);
        }

        private static void SetupVoteHistoryPagedMapping(VoterServiceFactory factory)
        {
            factory.Mapper.Setup(x => x.Map<PagedResponse<VoteHistoryResponse>>(It.IsAny<PagedList<Vote>>()))
                .Returns((PagedList<Vote> votes) => new PagedResponse<VoteHistoryResponse>
                {
                    Items = votes.Select(vote => new VoteHistoryResponse
                    {
                        VoteId = vote.Id,
                        ElectionId = vote.ElectionPosition.ElectionId,
                        ElectionName = vote.ElectionPosition.Election.Name,
                        ElectionPositionId = vote.ElectionPositionId,
                        PositionName = vote.ElectionPosition.Position.Name,
                        ContestantId = vote.ContestantId,
                        ContestantName = $"{vote.Contestant.PositionApplication.Student.User!.FirstName} {vote.Contestant.PositionApplication.Student.User.LastName}",
                        VotedAt = vote.VotedAt
                    }).ToList(),
                    MetaData = votes.MetaData
                });
        }

        private static async Task<Student> CreateStudentInSameDepartment(VoterServiceFactory factory, Student existingStudent, string firstName = "Jane", string lastName = "Doe")
        {
            string email = $"{firstName.ToLower()}.{lastName.ToLower()}.{Guid.NewGuid():N}@example.com";

            User user = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                Active = true
            };

            Student student = new()
            {
                Id = Guid.NewGuid(),
                RegNumber = $"REG-{Guid.NewGuid():N}",
                UserId = user.Id,
                User = user,
                DepartmentId = existingStudent.DepartmentId,
                Department = existingStudent.Department,
                GenderId = 1,
                Active = true
            };

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Students.Add(student);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            return student;
        }

        private static async Task<Contestant> CreateContestant(VoterServiceFactory factory, Student student, ElectionPosition electionPosition, bool active = true)
        {
            PositionApplication positionApplication = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionPositionId = electionPosition.Id,
                PositionApplicationStatusId = 1,
                Active = true
            };

            Contestant contestant = new()
            {
                Id = Guid.NewGuid().ToString(),
                PositionApplicationId = positionApplication.Id,
                Active = active,
                PositionApplication = positionApplication
            };

            factory.DbContextFactory.Context.PositionApplications.Add(positionApplication);
            factory.DbContextFactory.Context.Contestants.Add(contestant);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            return contestant;
        }

        private static async Task<RegisteredVoter> CreateRegisteredVoter(VoterServiceFactory factory, Student student, Election election, bool active = true)
        {
            RegisteredVoter registeredVoter = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                ElectionId = election.Id,
                VotingCode = $"VOTE-{Guid.NewGuid():N}",
                Active = active
            };

            factory.DbContextFactory.Context.RegisteredVoters.Add(registeredVoter);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            return registeredVoter;
        }

        private static async Task<Election> CreateVotingElection(VoterServiceFactory factory, bool active = true, DateTime? votingStartAt = null, DateTime? votingEndAt = null, bool configureVotingPeriod = true,
            int electionTypeId = 1, long yearId = 1, string name = "Student Election 2026")
        {
            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                ElectionTypeId = electionTypeId,
                YearId = yearId,
                VotingStartAt = configureVotingPeriod ? votingStartAt ?? DateTime.UtcNow.AddHours(-1) : null,
                VotingEndAt = configureVotingPeriod ? votingEndAt ?? DateTime.UtcNow.AddHours(1) : null,
                Active = active
            };

            factory.DbContextFactory.Context.Elections.Add(election);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            return election;
        }

        private static async Task<ElectionPosition> CreateElectionPosition(VoterServiceFactory factory, Election election, bool active = true, string positionName = "President")
        {
            Position position = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = positionName,
                Active = true
            };

            ElectionPosition electionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = position.Id,
                ApplicationFee = 0,
                Currency = "NGN",
                Active = active,
                Election = election,
                Position = position
            };

            factory.DbContextFactory.Context.Positions.Add(position);
            factory.DbContextFactory.Context.ElectionPositions.Add(electionPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            return electionPosition;
        }

        private static void SetupRegisteredVotersPagedMapping(VoterServiceFactory factory)
        {
            factory.Mapper.Setup(x => x.Map<PagedResponse<RegisteredVoterResponse>>(It.IsAny<PagedList<RegisteredVoter>>()))
                .Returns((PagedList<RegisteredVoter> registeredVoters) => new PagedResponse<RegisteredVoterResponse>
                {
                    Items = registeredVoters.Select(registeredVoter => new RegisteredVoterResponse
                    {
                        RegisteredVoterId = registeredVoter.Id,
                        StudentId = registeredVoter.StudentId,
                        RegistrationNumber = registeredVoter.Student.RegNumber ?? string.Empty,
                        StudentName = $"{registeredVoter.Student.User!.FirstName} {registeredVoter.Student.User.LastName}",
                        ElectionId = registeredVoter.ElectionId,
                        ElectionName = registeredVoter.Election.Name,
                        VotingCode = registeredVoter.VotingCode,
                        Active = registeredVoter.Active
                    }).ToList(),
                    MetaData = registeredVoters.MetaData
                });
        }

        private static async Task<Student> CreateStudent(VoterServiceFactory factory)
        {
            User user = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                UserName = "john.doe@example.com",
                Active = true
            };

            Faculty faculty = new()
            {
                Name = "Engineering",
                Active = true
            };

            Department department = new()
            {
                Name = "Computer Engineering",
                Faculty = faculty,
                Active = true
            };

            Student student = new()
            {
                Id = Guid.NewGuid(),
                RegNumber = $"REG-{Guid.NewGuid():N}",
                UserId = user.Id,
                User = user,
                Department = department,
                GenderId = 1,
                Active = true
            };

            factory.DbContextFactory.Context.Users.Add(user);
            factory.DbContextFactory.Context.Faculties.Add(faculty);
            factory.DbContextFactory.Context.Departments.Add(department);
            factory.DbContextFactory.Context.Students.Add(student);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            return student;
        }

        private static async Task<Election> CreateElection(VoterServiceFactory factory, bool active = true, string name = "Student Election 2026", int electionTypeId = 0, long yearId = 0)
        {
            Election election = new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                ElectionTypeId = electionTypeId,
                YearId = yearId,
                VoterRegistrationStartAt = DateTime.UtcNow.AddHours(-1),
                VoterRegistrationEndAt = DateTime.UtcNow.AddHours(1),
                Active = active
            };

            factory.DbContextFactory.Context.Elections.Add(election);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            return election;
        }

        private static void SetupRegisteredVoterMapping(VoterServiceFactory factory, Student student, Election election)
        {
            factory.Mapper.Setup(x => x.Map<RegisteredVoterResponse>(It.IsAny<RegisteredVoter>()))
                .Returns((RegisteredVoter registeredVoter) => new RegisteredVoterResponse
                {
                    RegisteredVoterId = registeredVoter.Id,
                    StudentId = registeredVoter.StudentId,
                    RegistrationNumber = student.RegNumber!,
                    StudentName = $"{student.User!.FirstName} {student.User.LastName}",
                    ElectionId = registeredVoter.ElectionId,
                    ElectionName = election.Name,
                    VotingCode = registeredVoter.VotingCode,
                    Active = registeredVoter.Active
                });
        }
    }
}