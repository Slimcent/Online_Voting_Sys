using Moq;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class ElectionPositionServiceTests
    {
        [Fact]
        public async Task GetElectionPositions_WithExistingPositions_ShouldReturnPagedResponse()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition presidentPosition = ElectionPositionTestData.CreateElectionPosition(election, president);
            ElectionPosition secretaryPosition = ElectionPositionTestData.CreateElectionPosition(election, secretary);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(presidentPosition, secretaryPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionResponse>> result = await factory.Service.GetElectionPositions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Equal(2, result.Value.Items.Count());
            Assert.Equal(2, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetElectionPositions_WithElectionId_ShouldReturnOnlyPositionsForElection()
        {
            using ElectionPositionServiceFactory factory = new();

            Election firstElection = ElectionTestData.CreateElection("First Election", departmentId: 1);
            Election secondElection = ElectionTestData.CreateElection("Second Election", departmentId: 2);

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition firstElectionPosition = ElectionPositionTestData.CreateElectionPosition(firstElection, president);
            ElectionPosition secondElectionPosition = ElectionPositionTestData.CreateElectionPosition(secondElection, secretary);

            await factory.DbContextFactory.Context.Set<Election>().AddRangeAsync(firstElection, secondElection);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(firstElectionPosition, secondElectionPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = firstElection.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionResponse>> result = await factory.Service.GetElectionPositions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionResponse response = Assert.Single(result.Value.Items);

            Assert.Equal(firstElection.Id, response.ElectionId);
            Assert.Equal("President", response.Position);
        }

        [Fact]
        public async Task GetElectionPositions_WithSearchTerm_ShouldReturnMatchingPositions()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition presidentPosition = ElectionPositionTestData.CreateElectionPosition(election, president);
            ElectionPosition secretaryPosition = ElectionPositionTestData.CreateElectionPosition(election, secretary);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(presidentPosition, secretaryPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                SearchTerm = "President",
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionResponse>> result = await factory.Service.GetElectionPositions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionResponse response = Assert.Single(result.Value.Items);

            Assert.Equal("President", response.Position);
        }

        [Fact]
        public async Task GetElectionPositions_WithActiveFilter_ShouldReturnMatchingPositions()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition activePosition = ElectionPositionTestData.CreateElectionPosition(election, president);
            ElectionPosition inactivePosition = ElectionPositionTestData.CreateElectionPosition(election, secretary, active: false);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(activePosition, inactivePosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                ElectionPositionActive = true,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionResponse>> result = await factory.Service.GetElectionPositions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionResponse response = Assert.Single(result.Value.Items);

            Assert.True(response.Active);
            Assert.Equal("President", response.Position);
        }

        [Fact]
        public async Task GetElectionPositions_WithApplications_ShouldReturnCorrectApplicationCounts()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition presidentPosition = ElectionPositionTestData.CreateElectionPosition(election, president);
            ElectionPosition secretaryPosition = ElectionPositionTestData.CreateElectionPosition(election, secretary);

            presidentPosition.Applications.Add(ElectionPositionTestData.CreatePositionApplication(presidentPosition));
            presidentPosition.Applications.Add(ElectionPositionTestData.CreatePositionApplication(presidentPosition));
            secretaryPosition.Applications.Add(ElectionPositionTestData.CreatePositionApplication(secretaryPosition));

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(presidentPosition, secretaryPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionResponse>> result = await factory.Service.GetElectionPositions(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionResponse presidentResponse = Assert.Single(result.Value.Items.Where(x => x.Position == "President"));
            ElectionPositionResponse secretaryResponse = Assert.Single(result.Value.Items.Where(x => x.Position == "Secretary"));

            Assert.Equal(2, presidentResponse.NumberOfApplications);
            Assert.Equal(1, secretaryResponse.NumberOfApplications);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithExistingApplications_ShouldReturnApplications()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            Position president = ElectionPositionTestData.CreatePosition("President");

            ElectionPosition electionPosition = ElectionPositionTestData.CreateElectionPosition(election, president);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering", faculty.Id);
            department.Id = 1;
            department.Faculty = faculty;

            User user = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "John",
                LastName = "Doe",
                UserTypeId = 1,
                Active = true
            };

            Student student = new()
            {
                Id = Guid.NewGuid(),
                RegNumber = "CE/2026/001",
                UserId = user.Id,
                User = user,
                DepartmentId = department.Id,
                Department = department,
                GenderId = 1,
                Active = true
            };

            PositionApplicationStatus applicationStatus = new()
            {
                Id = 3,
                Name = "Approved",
                Active = true
            };

            PositionApplication application = new()
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = student.Id,
                Student = student,
                ElectionPositionId = electionPosition.Id,
                ElectionPosition = electionPosition,
                PositionApplicationStatusId = applicationStatus.Id,
                PositionApplicationStatus = applicationStatus,
                Active = true
            };

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddAsync(president);
            await factory.DbContextFactory.Context.Set<Faculty>().AddAsync(faculty);
            await factory.DbContextFactory.Context.Set<Department>().AddAsync(department);
            await factory.DbContextFactory.Context.Set<User>().AddAsync(user);
            await factory.DbContextFactory.Context.Set<Student>().AddAsync(student);
            await factory.DbContextFactory.Context.Set<PositionApplicationStatus>().AddAsync(applicationStatus);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddAsync(electionPosition);
            await factory.DbContextFactory.Context.Set<PositionApplication>().AddAsync(application);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result =
                await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionWithApplicationsResponse response = Assert.Single(result.Value.Items);

            Assert.Equal(electionPosition.Id, response.Id);
            Assert.Equal("President", response.Position);
            Assert.Equal(1, response.NumberOfApplications);

            PositionApplicationResponse applicationResponse = Assert.Single(response.Applications);

            Assert.Equal(application.Id, applicationResponse.Id);
            Assert.Equal(student.Id, applicationResponse.StudentId);
            Assert.Equal("John", applicationResponse.FirstName);
            Assert.Equal("Doe", applicationResponse.LastName);
            Assert.Equal("CE/2026/001", applicationResponse.RegNumber);
            Assert.Equal(department.Id, applicationResponse.DepartmentId);
            Assert.Equal("Computer Engineering", applicationResponse.Department);
            Assert.Equal(faculty.Id, applicationResponse.FacultyId);
            Assert.Equal("Engineering", applicationResponse.Faculty);
            Assert.Equal(applicationStatus.Id, applicationResponse.PositionApplicationStatusId);
            Assert.Equal("Approved", applicationResponse.PositionApplicationStatus);
            Assert.True(applicationResponse.Active);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithCombinedApplicationFilters_ShouldRequireSameApplicationToMatch()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();
            Position president = ElectionPositionTestData.CreatePosition("President");
            ElectionPosition electionPosition = ElectionPositionTestData.CreateElectionPosition(election, president);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            Department computerEngineering = DepartmentTestData.CreateDepartment("Computer Engineering", faculty.Id);
            computerEngineering.Id = 1;
            computerEngineering.Faculty = faculty;

            Department electricalEngineering = DepartmentTestData.CreateDepartment("Electrical Engineering", faculty.Id);
            electricalEngineering.Id = 2;
            electricalEngineering.Faculty = faculty;

            User firstUser = ElectionPositionTestData.CreateUser("John", "Doe");
            User secondUser = ElectionPositionTestData.CreateUser("Jane", "Doe");

            Student firstStudent = ElectionPositionTestData.CreateStudent(firstUser, computerEngineering, "CE/2026/001");
            Student secondStudent = ElectionPositionTestData.CreateStudent(secondUser, electricalEngineering, "EE/2026/001");

            PositionApplicationStatus approvedStatus = ElectionPositionTestData.CreatePositionApplicationStatus(3, "Approved");
            PositionApplicationStatus rejectedStatus = ElectionPositionTestData.CreatePositionApplicationStatus(4, "Rejected");

            PositionApplication approvedApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, firstStudent, approvedStatus);
            PositionApplication rejectedApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, secondStudent, rejectedStatus);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddAsync(president);
            await factory.DbContextFactory.Context.Set<Faculty>().AddAsync(faculty);
            await factory.DbContextFactory.Context.Set<Department>().AddRangeAsync(computerEngineering, electricalEngineering);
            await factory.DbContextFactory.Context.Set<User>().AddRangeAsync(firstUser, secondUser);
            await factory.DbContextFactory.Context.Set<Student>().AddRangeAsync(firstStudent, secondStudent);
            await factory.DbContextFactory.Context.Set<PositionApplicationStatus>().AddRangeAsync(approvedStatus, rejectedStatus);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddAsync(electionPosition);
            await factory.DbContextFactory.Context.Set<PositionApplication>().AddRangeAsync(approvedApplication, rejectedApplication);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                PositionApplicationStatusId = approvedStatus.Id,
                DepartmentId = electricalEngineering.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.Items);
            Assert.Equal(0, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithNoPositions_ShouldReturnEmptyPagedResponse()
        {
            using ElectionPositionServiceFactory factory = new();

            ElectionPositionRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.Items);
            Assert.Equal(0, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithElectionId_ShouldReturnOnlyPositionsForElection()
        {
            using ElectionPositionServiceFactory factory = new();

            Election firstElection = ElectionTestData.CreateElection("First Election", departmentId: 1);
            Election secondElection = ElectionTestData.CreateElection("Second Election", departmentId: 2);

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition firstElectionPosition = ElectionPositionTestData.CreateElectionPosition(firstElection, president);
            ElectionPosition secondElectionPosition = ElectionPositionTestData.CreateElectionPosition(secondElection, secretary);

            await factory.DbContextFactory.Context.Set<Election>().AddRangeAsync(firstElection, secondElection);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(firstElectionPosition, secondElectionPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = firstElection.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionWithApplicationsResponse response = Assert.Single(result.Value.Items);

            Assert.Equal(firstElectionPosition.Id, response.Id);
            Assert.Equal(firstElection.Id, response.ElectionId);
            Assert.Equal("First Election", response.Election);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithPositionId_ShouldReturnMatchingPosition()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition presidentPosition = ElectionPositionTestData.CreateElectionPosition(election, president);
            ElectionPosition secretaryPosition = ElectionPositionTestData.CreateElectionPosition(election, secretary);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(presidentPosition, secretaryPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                PositionId = president.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionWithApplicationsResponse response = Assert.Single(result.Value.Items);

            Assert.Equal(presidentPosition.Id, response.Id);
            Assert.Equal(president.Id, response.PositionId);
            Assert.Equal("President", response.Position);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithActiveFilter_ShouldReturnMatchingPositions()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition activePosition = ElectionPositionTestData.CreateElectionPosition(election, president);
            ElectionPosition inactivePosition = ElectionPositionTestData.CreateElectionPosition(election, secretary, active: false);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(activePosition, inactivePosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                ElectionPositionActive = true,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionWithApplicationsResponse response = Assert.Single(result.Value.Items);

            Assert.Equal(activePosition.Id, response.Id);
            Assert.True(response.Active);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithSearchTerm_ShouldReturnMatchingPositions()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition presidentPosition = ElectionPositionTestData.CreateElectionPosition(election, president);
            ElectionPosition secretaryPosition = ElectionPositionTestData.CreateElectionPosition(election, secretary);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(presidentPosition, secretaryPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                SearchTerm = "President",
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionWithApplicationsResponse response = Assert.Single(result.Value.Items);

            Assert.Equal(presidentPosition.Id, response.Id);
            Assert.Equal("President", response.Position);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithStatusFilter_ShouldReturnOnlyMatchingApplications()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();
            Position president = ElectionPositionTestData.CreatePosition("President");
            ElectionPosition electionPosition = ElectionPositionTestData.CreateElectionPosition(election, president);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering", faculty.Id);
            department.Id = 1;
            department.Faculty = faculty;

            User firstUser = ElectionPositionTestData.CreateUser("John", "Doe");
            User secondUser = ElectionPositionTestData.CreateUser("Jane", "Doe");

            Student firstStudent = ElectionPositionTestData.CreateStudent(firstUser, department, "CE/2026/001");
            Student secondStudent = ElectionPositionTestData.CreateStudent(secondUser, department, "CE/2026/002");

            PositionApplicationStatus approvedStatus = ElectionPositionTestData.CreatePositionApplicationStatus(3, "Approved");
            PositionApplicationStatus rejectedStatus = ElectionPositionTestData.CreatePositionApplicationStatus(4, "Rejected");

            PositionApplication approvedApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, firstStudent, approvedStatus);
            PositionApplication rejectedApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, secondStudent, rejectedStatus);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddAsync(president);
            await factory.DbContextFactory.Context.Set<Faculty>().AddAsync(faculty);
            await factory.DbContextFactory.Context.Set<Department>().AddAsync(department);
            await factory.DbContextFactory.Context.Set<User>().AddRangeAsync(firstUser, secondUser);
            await factory.DbContextFactory.Context.Set<Student>().AddRangeAsync(firstStudent, secondStudent);
            await factory.DbContextFactory.Context.Set<PositionApplicationStatus>().AddRangeAsync(approvedStatus, rejectedStatus);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddAsync(electionPosition);
            await factory.DbContextFactory.Context.Set<PositionApplication>().AddRangeAsync(approvedApplication, rejectedApplication);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                PositionApplicationStatusId = approvedStatus.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionWithApplicationsResponse response = Assert.Single(result.Value.Items);
            PositionApplicationResponse application = Assert.Single(response.Applications);

            Assert.Equal(1, response.NumberOfApplications);
            Assert.Equal(approvedApplication.Id, application.Id);
            Assert.Equal(approvedStatus.Id, application.PositionApplicationStatusId);
            Assert.Equal("Approved", application.PositionApplicationStatus);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithDepartmentFilter_ShouldReturnOnlyMatchingApplications()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();
            Position president = ElectionPositionTestData.CreatePosition("President");
            ElectionPosition electionPosition = ElectionPositionTestData.CreateElectionPosition(election, president);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            Department computerEngineering = DepartmentTestData.CreateDepartment("Computer Engineering", faculty.Id);
            computerEngineering.Id = 1;
            computerEngineering.Faculty = faculty;

            Department electricalEngineering = DepartmentTestData.CreateDepartment("Electrical Engineering", faculty.Id);
            electricalEngineering.Id = 2;
            electricalEngineering.Faculty = faculty;

            User firstUser = ElectionPositionTestData.CreateUser("John", "Doe");
            User secondUser = ElectionPositionTestData.CreateUser("Jane", "Doe");

            Student firstStudent = ElectionPositionTestData.CreateStudent(firstUser, computerEngineering, "CE/2026/001");
            Student secondStudent = ElectionPositionTestData.CreateStudent(secondUser, electricalEngineering, "EE/2026/001");

            PositionApplicationStatus status = ElectionPositionTestData.CreatePositionApplicationStatus();

            PositionApplication firstApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, firstStudent, status);
            PositionApplication secondApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, secondStudent, status);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddAsync(president);
            await factory.DbContextFactory.Context.Set<Faculty>().AddAsync(faculty);
            await factory.DbContextFactory.Context.Set<Department>().AddRangeAsync(computerEngineering, electricalEngineering);
            await factory.DbContextFactory.Context.Set<User>().AddRangeAsync(firstUser, secondUser);
            await factory.DbContextFactory.Context.Set<Student>().AddRangeAsync(firstStudent, secondStudent);
            await factory.DbContextFactory.Context.Set<PositionApplicationStatus>().AddAsync(status);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddAsync(electionPosition);
            await factory.DbContextFactory.Context.Set<PositionApplication>().AddRangeAsync(firstApplication, secondApplication);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                DepartmentId = computerEngineering.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionWithApplicationsResponse response = Assert.Single(result.Value.Items);
            PositionApplicationResponse application = Assert.Single(response.Applications);

            Assert.Equal(1, response.NumberOfApplications);
            Assert.Equal(firstApplication.Id, application.Id);
            Assert.Equal(computerEngineering.Id, application.DepartmentId);
            Assert.Equal("Computer Engineering", application.Department);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithFacultyFilter_ShouldReturnOnlyMatchingApplications()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();
            Position president = ElectionPositionTestData.CreatePosition("President");
            ElectionPosition electionPosition = ElectionPositionTestData.CreateElectionPosition(election, president);

            Faculty engineering = FacultyTestData.CreateFaculty("Engineering");
            engineering.Id = 1;

            Faculty science = FacultyTestData.CreateFaculty("Science");
            science.Id = 2;

            Department computerEngineering = DepartmentTestData.CreateDepartment("Computer Engineering", engineering.Id);
            computerEngineering.Id = 1;
            computerEngineering.Faculty = engineering;

            Department computerScience = DepartmentTestData.CreateDepartment("Computer Science", science.Id);
            computerScience.Id = 2;
            computerScience.Faculty = science;

            User firstUser = ElectionPositionTestData.CreateUser("John", "Doe");
            User secondUser = ElectionPositionTestData.CreateUser("Jane", "Doe");

            Student firstStudent = ElectionPositionTestData.CreateStudent(firstUser, computerEngineering, "CE/2026/001");
            Student secondStudent = ElectionPositionTestData.CreateStudent(secondUser, computerScience, "CS/2026/001");

            PositionApplicationStatus status = ElectionPositionTestData.CreatePositionApplicationStatus();

            PositionApplication firstApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, firstStudent, status);
            PositionApplication secondApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, secondStudent, status);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddAsync(president);
            await factory.DbContextFactory.Context.Set<Faculty>().AddRangeAsync(engineering, science);
            await factory.DbContextFactory.Context.Set<Department>().AddRangeAsync(computerEngineering, computerScience);
            await factory.DbContextFactory.Context.Set<User>().AddRangeAsync(firstUser, secondUser);
            await factory.DbContextFactory.Context.Set<Student>().AddRangeAsync(firstStudent, secondStudent);
            await factory.DbContextFactory.Context.Set<PositionApplicationStatus>().AddAsync(status);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddAsync(electionPosition);
            await factory.DbContextFactory.Context.Set<PositionApplication>().AddRangeAsync(firstApplication, secondApplication);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                FacultyId = engineering.Id,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionWithApplicationsResponse response = Assert.Single(result.Value.Items);
            PositionApplicationResponse application = Assert.Single(response.Applications);

            Assert.Equal(1, response.NumberOfApplications);
            Assert.Equal(firstApplication.Id, application.Id);
            Assert.Equal(engineering.Id, application.FacultyId);
            Assert.Equal("Engineering", application.Faculty);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithApplicationActiveFilter_ShouldReturnOnlyMatchingApplications()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();
            Position president = ElectionPositionTestData.CreatePosition("President");
            ElectionPosition electionPosition = ElectionPositionTestData.CreateElectionPosition(election, president);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering", faculty.Id);
            department.Id = 1;
            department.Faculty = faculty;

            User firstUser = ElectionPositionTestData.CreateUser("John", "Doe");
            User secondUser = ElectionPositionTestData.CreateUser("Jane", "Doe");

            Student firstStudent = ElectionPositionTestData.CreateStudent(firstUser, department, "CE/2026/001");
            Student secondStudent = ElectionPositionTestData.CreateStudent(secondUser, department, "CE/2026/002");

            PositionApplicationStatus status = ElectionPositionTestData.CreatePositionApplicationStatus();

            PositionApplication activeApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, firstStudent, status);
            PositionApplication inactiveApplication = ElectionPositionTestData.CreatePositionApplication(electionPosition, secondStudent, status, false);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddAsync(president);
            await factory.DbContextFactory.Context.Set<Faculty>().AddAsync(faculty);
            await factory.DbContextFactory.Context.Set<Department>().AddAsync(department);
            await factory.DbContextFactory.Context.Set<User>().AddRangeAsync(firstUser, secondUser);
            await factory.DbContextFactory.Context.Set<Student>().AddRangeAsync(firstStudent, secondStudent);
            await factory.DbContextFactory.Context.Set<PositionApplicationStatus>().AddAsync(status);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddAsync(electionPosition);
            await factory.DbContextFactory.Context.Set<PositionApplication>().AddRangeAsync(activeApplication, inactiveApplication);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                ApplicationActive = true,
                PageNumber = 1,
                PageSize = 10
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            ElectionPositionWithApplicationsResponse response = Assert.Single(result.Value.Items);
            PositionApplicationResponse application = Assert.Single(response.Applications);

            Assert.Equal(1, response.NumberOfApplications);
            Assert.Equal(activeApplication.Id, application.Id);
            Assert.True(application.Active);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithPagination_ShouldReturnApplicationsOnlyForCurrentPage()
        {
            using ElectionPositionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            Position president = ElectionPositionTestData.CreatePosition("President");
            Position secretary = ElectionPositionTestData.CreatePosition("Secretary");

            ElectionPosition presidentPosition = ElectionPositionTestData.CreateElectionPosition(election, president);
            ElectionPosition secretaryPosition = ElectionPositionTestData.CreateElectionPosition(election, secretary);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering", faculty.Id);
            department.Id = 1;
            department.Faculty = faculty;

            User firstUser = ElectionPositionTestData.CreateUser("John", "Doe");
            User secondUser = ElectionPositionTestData.CreateUser("Jane", "Doe");

            Student firstStudent = ElectionPositionTestData.CreateStudent(firstUser, department, "CE/2026/001");
            Student secondStudent = ElectionPositionTestData.CreateStudent(secondUser, department, "CE/2026/002");

            PositionApplicationStatus status = ElectionPositionTestData.CreatePositionApplicationStatus();

            PositionApplication presidentApplication = ElectionPositionTestData.CreatePositionApplication(presidentPosition, firstStudent, status);
            PositionApplication secretaryApplication = ElectionPositionTestData.CreatePositionApplication(secretaryPosition, secondStudent, status);

            await factory.DbContextFactory.Context.Set<Election>().AddAsync(election);
            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(president, secretary);
            await factory.DbContextFactory.Context.Set<Faculty>().AddAsync(faculty);
            await factory.DbContextFactory.Context.Set<Department>().AddAsync(department);
            await factory.DbContextFactory.Context.Set<User>().AddRangeAsync(firstUser, secondUser);
            await factory.DbContextFactory.Context.Set<Student>().AddRangeAsync(firstStudent, secondStudent);
            await factory.DbContextFactory.Context.Set<PositionApplicationStatus>().AddAsync(status);
            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddRangeAsync(presidentPosition, secretaryPosition);
            await factory.DbContextFactory.Context.Set<PositionApplication>().AddRangeAsync(presidentApplication, secretaryApplication);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            ElectionPositionRequest request = new()
            {
                ElectionId = election.Id,
                PageNumber = 1,
                PageSize = 1
            };

            Result<PagedResponse<ElectionPositionWithApplicationsResponse>> result = await factory.Service.GetElectionPositionsWithApplications(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);
            Assert.Equal(2, result.Value.MetaData.TotalCount);

            ElectionPositionWithApplicationsResponse response = result.Value.Items.Single();
            PositionApplicationResponse application = Assert.Single(response.Applications);

            Assert.Equal(presidentPosition.Id, response.Id);
            Assert.Equal(presidentApplication.Id, application.Id);
            Assert.Equal(1, response.NumberOfApplications);
        }

        [Fact]
        public async Task CreateElectionPosition_WithValidRequest_ShouldCreateElectionPosition()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();

            Election election = ElectionPositionTestData.CreateElection(request.ElectionId);
            Position position = ElectionPositionTestData.CreatePosition(request.PositionId);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(election);

            factory.PositionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()))
                .ReturnsAsync(position);

            factory.ElectionPositionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(false);

            Result<string> result = await factory.Service.CreateElectionPosition(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("Election position created successfully.", result.Value);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionPosition>(request), Times.Once);

            factory.ElectionPositionRepository.Verify(repository => repository.AddAsync(It.Is<ElectionPosition>(electionPosition =>
                    electionPosition.ElectionId == request.ElectionId
                    && electionPosition.PositionId == request.PositionId
                    && electionPosition.ApplicationFee == request.ApplicationFee
                    && electionPosition.Currency == request.Currency),
                It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task CreateElectionPosition_WithMissingElection_ShouldReturnNotFound()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync((Election?)null);

            Result<string> result = await factory.Service.CreateElectionPosition(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election with id {request.ElectionId} was not found.", result.Error);

            factory.PositionRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionPosition>(It.IsAny<CreateElectionPositionRequest>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.AddAsync(It.IsAny<ElectionPosition>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElectionPosition_WithMissingPosition_ShouldReturnNotFound()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();

            Election election = ElectionPositionTestData.CreateElection(request.ElectionId);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(election);

            factory.PositionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()))
                .ReturnsAsync((Position?)null);

            Result<string> result = await factory.Service.CreateElectionPosition(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Position with id {request.PositionId} was not found.", result.Error);

            factory.ElectionPositionRepository.Verify(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionPosition>(It.IsAny<CreateElectionPositionRequest>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.AddAsync(It.IsAny<ElectionPosition>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElectionPosition_WithExistingElectionPosition_ShouldReturnConflict()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();

            Election election = ElectionPositionTestData.CreateElection(request.ElectionId);
            Position position = ElectionPositionTestData.CreatePositionWithId(request.PositionId);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(election);

            factory.PositionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()))
                .ReturnsAsync(position);

            factory.ElectionPositionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.CreateElectionPosition(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("The position is already assigned to the election.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionPosition>(It.IsAny<CreateElectionPositionRequest>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.AddAsync(It.IsAny<ElectionPosition>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElectionPositions_WithValidRequest_ShouldCreateElectionPositions()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionsRequest request = ElectionPositionTestData.CreateElectionPositionsRequest();

            Election election = ElectionPositionTestData.CreateElection(request.ElectionId);

            List<Position> positions = request.ElectionPositions
                .Select((item, index) => ElectionPositionTestData.CreatePositionWithId(item.PositionId, $"Position {index + 1}"))
                .ToList();

            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(positions);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(election);

            Result<string> result = await factory.Service.CreateElectionPositions(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("3 election positions created successfully.", result.Value);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionPosition>(
                It.IsAny<CreateElectionPositionItemRequest>()), Times.Exactly(3));

            factory.ElectionPositionRepository.Verify(repository => repository.AddRangeAsync(It.Is<IEnumerable<ElectionPosition>>(electionPositions =>
                    electionPositions.Count() == 3
                    && electionPositions.All(electionPosition => electionPosition.ElectionId == request.ElectionId)
                    && request.ElectionPositions.All(item => electionPositions.Any(electionPosition =>
                        electionPosition.PositionId == item.PositionId
                        && electionPosition.ApplicationFee == item.ApplicationFee
                        && electionPosition.Currency == item.Currency)))),
                Times.Once);
        }

        [Fact]
        public async Task CreateElectionPositions_WithMissingElection_ShouldReturnNotFound()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionsRequest request = ElectionPositionTestData.CreateElectionPositionsRequest();

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync((Election?)null);

            Result<string> result = await factory.Service.CreateElectionPositions(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election with id {request.ElectionId} was not found.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionPosition>(
                It.IsAny<CreateElectionPositionItemRequest>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<ElectionPosition>>()), Times.Never);
        }

        [Fact]
        public async Task CreateElectionPositions_WithMissingPosition_ShouldReturnNotFound()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionsRequest request = ElectionPositionTestData.CreateElectionPositionsRequest();

            Election election = ElectionPositionTestData.CreateElection(request.ElectionId);

            List<Position> positions = request.ElectionPositions
                .Take(2)
                .Select((item, index) => ElectionPositionTestData.CreatePositionWithId(item.PositionId, $"Position {index + 1}"))
                .ToList();

            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(positions);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(election);

            string missingPositionId = request.ElectionPositions[2].PositionId;

            Result<string> result = await factory.Service.CreateElectionPositions(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Position with id {missingPositionId} was not found.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionPosition>(It.IsAny<CreateElectionPositionItemRequest>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<ElectionPosition>>()), Times.Never);
        }

        [Fact]
        public async Task CreateElectionPositions_WithExistingElectionPosition_ShouldReturnConflict()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionsRequest request = ElectionPositionTestData.CreateElectionPositionsRequest();

            Election election = ElectionPositionTestData.CreateElection(request.ElectionId);

            List<Position> positions = request.ElectionPositions
                .Select((item, index) => ElectionPositionTestData.CreatePositionWithId(item.PositionId, $"Position {index + 1}"))
                .ToList();

            await factory.DbContextFactory.Context.Set<Position>().AddRangeAsync(positions);

            ElectionPosition existingElectionPosition = new()
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = request.ElectionId,
                PositionId = request.ElectionPositions[0].PositionId,
                ApplicationFee = 1000,
                Currency = "NGN"
            };

            await factory.DbContextFactory.Context.Set<ElectionPosition>().AddAsync(existingElectionPosition);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(election);

            Result<string> result = await factory.Service.CreateElectionPositions(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("One or more positions are already assigned to the election.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionPosition>(It.IsAny<CreateElectionPositionItemRequest>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<ElectionPosition>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionPosition_WithValidRequest_ShouldUpdateElectionPosition()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();
            request.Id = Guid.NewGuid().ToString();

            ElectionPosition electionPosition = new()
            {
                Id = request.Id,
                ElectionId = Guid.NewGuid().ToString(),
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 500,
                Currency = "NGN"
            };

            Election election = ElectionPositionTestData.CreateElection(request.ElectionId);
            Position position = ElectionPositionTestData.CreatePositionWithId(request.PositionId);

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(electionPosition);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(election);

            factory.PositionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()))
                .ReturnsAsync(position);

            factory.ElectionPositionRepository.Setup(repository => repository.AnyAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(false);

            Result<string> result = await factory.Service.UpdateElectionPosition(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Election position updated successfully.", result.Value);

            factory.Mapper.Verify(mapper => mapper.Map(request, electionPosition), Times.Once);

            factory.ElectionPositionRepository.Verify(repository => repository.UpdateAsync(electionPosition), Times.Once);
        }

        [Fact]
        public async Task UpdateElectionPosition_WithMissingId_ShouldReturnValidationError()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();
            request.Id = null;

            Result<string> result = await factory.Service.UpdateElectionPosition(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Election position id is required.", result.Error);

            factory.ElectionPositionRepository.Verify(repository => repository.GetSingleByAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map(
                It.IsAny<CreateElectionPositionRequest>(),
                It.IsAny<ElectionPosition>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.UpdateAsync(
                It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionPosition_WithMissingElectionPosition_ShouldReturnNotFound()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();
            request.Id = Guid.NewGuid().ToString();

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync((ElectionPosition?)null);

            Result<string> result = await factory.Service.UpdateElectionPosition(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election position with id {request.Id} was not found.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.GetSingleByAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()), Times.Never);

            factory.PositionRepository.Verify(repository => repository.GetSingleByAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map(
                It.IsAny<CreateElectionPositionRequest>(),
                It.IsAny<ElectionPosition>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.UpdateAsync(
                It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionPosition_WithMissingElection_ShouldReturnNotFound()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();
            request.Id = Guid.NewGuid().ToString();

            ElectionPosition electionPosition = new()
            {
                Id = request.Id,
                ElectionId = Guid.NewGuid().ToString(),
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 500,
                Currency = "NGN"
            };

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(electionPosition);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync((Election?)null);

            Result<string> result = await factory.Service.UpdateElectionPosition(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election with id {request.ElectionId} was not found.", result.Error);

            factory.PositionRepository.Verify(repository => repository.GetSingleByAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map(
                It.IsAny<CreateElectionPositionRequest>(),
                It.IsAny<ElectionPosition>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.UpdateAsync(
                It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionPosition_WithMissingPosition_ShouldReturnNotFound()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();
            request.Id = Guid.NewGuid().ToString();

            ElectionPosition electionPosition = new()
            {
                Id = request.Id,
                ElectionId = Guid.NewGuid().ToString(),
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 500,
                Currency = "NGN"
            };

            Election election = ElectionPositionTestData.CreateElection(request.ElectionId);

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(electionPosition);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(election);

            factory.PositionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()))
                .ReturnsAsync((Position?)null);

            Result<string> result = await factory.Service.UpdateElectionPosition(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Position with id {request.PositionId} was not found.", result.Error);

            factory.ElectionPositionRepository.Verify(repository => repository.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map(
                It.IsAny<CreateElectionPositionRequest>(),
                It.IsAny<ElectionPosition>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.UpdateAsync(
                It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionPosition_WithExistingElectionPosition_ShouldReturnConflict()
        {
            using ElectionPositionServiceFactory factory = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();
            request.Id = Guid.NewGuid().ToString();

            ElectionPosition electionPosition = new()
            {
                Id = request.Id,
                ElectionId = Guid.NewGuid().ToString(),
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 500,
                Currency = "NGN"
            };

            Election election = ElectionPositionTestData.CreateElection(request.ElectionId);
            Position position = ElectionPositionTestData.CreatePositionWithId(request.PositionId);

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(electionPosition);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(election);

            factory.PositionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Position, bool>>>()))
                .ReturnsAsync(position);

            factory.ElectionPositionRepository.Setup(repository => repository.AnyAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.UpdateElectionPosition(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal($"Position with id {request.PositionId} is already assigned to election with id {request.ElectionId}.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map(
                It.IsAny<CreateElectionPositionRequest>(),
                It.IsAny<ElectionPosition>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task DeleteElectionPosition_WithMissingId_ShouldReturnValidationError()
        {
            using ElectionPositionServiceFactory factory = new();

            Result<string> result = await factory.Service.DeleteElectionPosition(string.Empty);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Election position id is required.", result.Error);

            factory.ElectionPositionRepository.Verify(repository => repository.GetSingleByAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()), Times.Never);

            factory.PositionApplicationRepository.Verify(repository => repository.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<PositionApplication, bool>>>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.DeleteAsync(It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task DeleteElectionPosition_WithMissingElectionPosition_ShouldReturnNotFound()
        {
            using ElectionPositionServiceFactory factory = new();

            string id = Guid.NewGuid().ToString();

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync((ElectionPosition?)null);

            Result<string> result = await factory.Service.DeleteElectionPosition(id);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election position with id {id} was not found.", result.Error);

            factory.PositionApplicationRepository.Verify(repository => repository.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<PositionApplication, bool>>>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.DeleteAsync(It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task DeleteElectionPosition_WithApplications_ShouldReturnConflict()
        {
            using ElectionPositionServiceFactory factory = new();

            string id = Guid.NewGuid().ToString();

            ElectionPosition electionPosition = new()
            {
                Id = id,
                ElectionId = Guid.NewGuid().ToString(),
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 1000,
                Currency = "NGN"
            };

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(electionPosition);

            factory.PositionApplicationRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PositionApplication, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.DeleteElectionPosition(id);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Election position cannot be deleted because it has applications.", result.Error);

            factory.ElectionPositionRepository.Verify(repository => repository.DeleteAsync(It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task DeleteElectionPosition_WithNoApplications_ShouldDeleteElectionPosition()
        {
            using ElectionPositionServiceFactory factory = new();

            string id = Guid.NewGuid().ToString();

            ElectionPosition electionPosition = new()
            {
                Id = id,
                ElectionId = Guid.NewGuid().ToString(),
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 1000,
                Currency = "NGN"
            };

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(electionPosition);

            factory.PositionApplicationRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PositionApplication, bool>>>()))
                .ReturnsAsync(false);

            Result<string> result = await factory.Service.DeleteElectionPosition(id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Election position deleted successfully.", result.Value);

            factory.ElectionPositionRepository.Verify(repository => repository.DeleteAsync(electionPosition), Times.Once);
        }

        [Fact]
        public async Task ToggleElectionPositionActivation_WithMissingId_ShouldReturnValidationError()
        {
            using ElectionPositionServiceFactory factory = new();

            Result<string> result = await factory.Service.ToggleElectionPositionActivation(string.Empty);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Election position id is required.", result.Error);

            factory.ElectionPositionRepository.Verify(repository => repository.GetSingleByAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()), Times.Never);

            factory.ElectionPositionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task ToggleElectionPositionActivation_WithMissingElectionPosition_ShouldReturnNotFound()
        {
            using ElectionPositionServiceFactory factory = new();

            string id = Guid.NewGuid().ToString();

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync((ElectionPosition?)null);

            Result<string> result = await factory.Service.ToggleElectionPositionActivation(id);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election position with id {id} was not found.", result.Error);

            factory.ElectionPositionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionPosition>()), Times.Never);
        }

        [Fact]
        public async Task ToggleElectionPositionActivation_WithExistingElectionPosition_ShouldToggleActivation()
        {
            using ElectionPositionServiceFactory factory = new();

            string id = Guid.NewGuid().ToString();

            ElectionPosition electionPosition = new()
            {
                Id = id,
                ElectionId = Guid.NewGuid().ToString(),
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 1000,
                Currency = "NGN",
                Active = true
            };

            factory.ElectionPositionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionPosition, bool>>>()))
                .ReturnsAsync(electionPosition);

            Result<string> result = await factory.Service.ToggleElectionPositionActivation(id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Election position deactivated successfully.", result.Value);
            Assert.False(electionPosition.Active);

            factory.ElectionPositionRepository.Verify(repository => repository.UpdateAsync(electionPosition), Times.Once);
        }
    }
}