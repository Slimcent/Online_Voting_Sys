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