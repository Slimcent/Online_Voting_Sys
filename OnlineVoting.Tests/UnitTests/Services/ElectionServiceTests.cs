using Moq;
using OnlineVoting.Api.Mapper.CustomResolvers;
using OnlineVoting.Models.Constants;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class ElectionServiceTests
    {
        [Fact]
        public async Task GetElectionStatuses_WithExistingStatuses_ShouldReturnElectionStatuses()
        {
            using ElectionServiceFactory factory = new();

            List<ElectionStatus> electionStatuses = ElectionStatusTestData.CreateElectionStatuses();

            await factory.DbContextFactory.Context.Set<ElectionStatus>().AddRangeAsync(electionStatuses);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(mapper => mapper.Map<IEnumerable<ElectionStatusResponse>>(It.IsAny<List<ElectionStatus>>()))
                .Returns((List<ElectionStatus> statuses) => statuses.Select(status => new ElectionStatusResponse
                {
                    Id = status.Id,
                    Code = status.Code,
                    Name = status.Name,
                    Description = status.Description,
                    Active = status.Active,
                    NumberOfElections = status.Elections.Count
                }));

            Result<IEnumerable<ElectionStatusResponse>> result = await factory.Service.GetElectionStatuses();

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(6, result.Value.Count());
        }

        [Fact]
        public async Task GetElectionStatuses_WithNoStatuses_ShouldReturnEmptyList()
        {
            using ElectionServiceFactory factory = new();

            factory.Mapper.Setup(mapper => mapper.Map<IEnumerable<ElectionStatusResponse>>(It.IsAny<List<ElectionStatus>>()))
                .Returns(Enumerable.Empty<ElectionStatusResponse>());

            Result<IEnumerable<ElectionStatusResponse>> result = await factory.Service.GetElectionStatuses();

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetElectionStatus_WithExistingStatus_ShouldReturnElectionStatus()
        {
            using ElectionServiceFactory factory = new();

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.ElectionStatusRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionStatus, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionStatus>, IOrderedQueryable<ElectionStatus>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionStatus>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionStatus, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionStatus);

            ElectionStatusResponse response = new()
            {
                Id = electionStatus.Id,
                Name = electionStatus.Name,
                Code = electionStatus.Code,
                Description = electionStatus.Description,
                Active = electionStatus.Active,
                NumberOfElections = 0
            };

            factory.Mapper.Setup(mapper => mapper.Map<ElectionStatusResponse>(electionStatus))
                .Returns(response);

            Result<ElectionStatusResponse> result = await factory.Service.GetElectionStatus(electionStatus.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(electionStatus.Id, result.Value.Id);
            Assert.Equal(electionStatus.Code, result.Value.Code);
            Assert.Equal(electionStatus.Name, result.Value.Name);
        }

        [Fact]
        public async Task GetElectionStatus_WithMissingStatus_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            factory.ElectionStatusRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionStatus, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionStatus>, IOrderedQueryable<ElectionStatus>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionStatus>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionStatus, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((ElectionStatus?)null);

            Result<ElectionStatusResponse> result = await factory.Service.GetElectionStatus(100);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Election status with id 100 was not found.", result.Error);
        }

        [Fact]
        public async Task UpdateElectionStatus_WithExistingStatus_ShouldUpdateElectionStatus()
        {
            using ElectionServiceFactory factory = new();

            UpdateElectionStatusRequest request = ElectionStatusTestData.CreateUpdateElectionStatusRequest();
            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.Id))
                .ReturnsAsync(electionStatus);

            factory.ElectionStatusRepository.Setup(repository => repository.AnyAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionStatus, bool>>>()))
                .ReturnsAsync(false);

            factory.Mapper.Setup(mapper => mapper.Map(request, electionStatus))
                .Returns(electionStatus);

            Result<string> result = await factory.Service.UpdateElectionStatus(request);

            Assert.Equal(ResultStatus.Success, result.Status);

            factory.Mapper.Verify(mapper => mapper.Map(request, electionStatus), Times.Once);
            factory.ElectionStatusRepository.Verify(repository => repository.UpdateAsync(electionStatus), Times.Once);
        }

        [Fact]
        public async Task UpdateElectionStatus_WithMissingStatus_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            UpdateElectionStatusRequest request = ElectionStatusTestData.CreateUpdateElectionStatusRequest();

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.Id))
                .ReturnsAsync((ElectionStatus?)null);

            Result<string> result = await factory.Service.UpdateElectionStatus(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election status with id {request.Id} was not found.", result.Error);

            factory.ElectionStatusRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionStatus>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionStatus_WithDuplicateName_ShouldReturnConflict()
        {
            using ElectionServiceFactory factory = new();

            UpdateElectionStatusRequest request = ElectionStatusTestData.CreateUpdateElectionStatusRequest();
            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.Id))
                .ReturnsAsync(electionStatus);

            factory.ElectionStatusRepository.Setup(repository => repository.AnyAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionStatus, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.UpdateElectionStatus(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal($"Election status with name {request.Name.Trim()} already exists.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map(request, electionStatus), Times.Never);
            factory.ElectionStatusRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionStatus>()), Times.Never);
        }

        [Fact]
        public async Task GetPagedElectionStatuses_WithoutSearchTerm_ShouldReturnPagedResponse()
        {
            using ElectionServiceFactory factory = new();

            ElectionStatusRequest request = new()
            {
                PageNumber = 1,
                PageSize = 4
            };

            List<ElectionStatus> electionStatuses = ElectionStatusTestData.CreateElectionStatuses();

            await factory.DbContextFactory.Context.Set<ElectionStatus>().AddRangeAsync(electionStatuses);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(mapper => mapper.Map<PagedResponse<ElectionStatusResponse>>(It.IsAny<PagedList<ElectionStatus>>()))
                .Returns((PagedList<ElectionStatus> statuses) => new PagedResponse<ElectionStatusResponse>
                {
                    Items = statuses.Select(status => new ElectionStatusResponse
                    {
                        Id = status.Id,
                        Code = status.Code,
                        Name = status.Name,
                        Description = status.Description,
                        Active = status.Active,
                        NumberOfElections = status.Elections.Count
                    }).ToList(),
                    MetaData = statuses.MetaData
                });

            Result<PagedResponse<ElectionStatusResponse>> result = await factory.Service.GetPagedElectionStatuses(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Equal(4, result.Value.Items.Count());
            Assert.Equal(6, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetPagedElectionStatuses_WithSearchTerm_ShouldReturnFilteredPagedResponse()
        {
            using ElectionServiceFactory factory = new();

            ElectionStatusRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Voting"
            };

            List<ElectionStatus> electionStatuses = ElectionStatusTestData.CreateElectionStatuses();

            await factory.DbContextFactory.Context.Set<ElectionStatus>().AddRangeAsync(electionStatuses);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(mapper => mapper.Map<PagedResponse<ElectionStatusResponse>>(It.IsAny<PagedList<ElectionStatus>>()))
                .Returns((PagedList<ElectionStatus> statuses) => new PagedResponse<ElectionStatusResponse>
                {
                    Items = statuses.Select(status => new ElectionStatusResponse
                    {
                        Id = status.Id,
                        Code = status.Code,
                        Name = status.Name,
                        Description = status.Description,
                        Active = status.Active,
                        NumberOfElections = status.Elections.Count
                    }).ToList(),
                    MetaData = statuses.MetaData
                });

            Result<PagedResponse<ElectionStatusResponse>> result = await factory.Service.GetPagedElectionStatuses(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);
            Assert.Equal(1, result.Value.Items.Count());
            Assert.Equal(1, result.Value.MetaData.TotalCount);
            Assert.All(result.Value.Items, status => Assert.Contains("Voting", status.Name));
        }

        [Fact]
        public async Task UpdateElectionScope_WithExistingScope_ShouldUpdateElectionScope()
        {
            using ElectionServiceFactory factory = new();

            UpdateElectionScopeRequest request = ElectionScopeTestData.CreateUpdateElectionScopeRequest();
            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope();

            factory.ElectionScopeRepository.Setup(repository => repository.GetByIdAsync(request.Id))
                .ReturnsAsync(electionScope);

            factory.ElectionScopeRepository.Setup(repository => repository.AnyAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionScope, bool>>>()))
                .ReturnsAsync(false);

            factory.Mapper.Setup(mapper => mapper.Map(request, electionScope))
                .Returns(electionScope);

            Result<string> result = await factory.Service.UpdateElectionScope(request);

            Assert.Equal(ResultStatus.Success, result.Status);

            factory.Mapper.Verify(mapper => mapper.Map(request, electionScope), Times.Once);
            factory.ElectionScopeRepository.Verify(repository => repository.UpdateAsync(electionScope), Times.Once);
        }

        [Fact]
        public async Task UpdateElectionScope_WithMissingScope_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            UpdateElectionScopeRequest request = ElectionScopeTestData.CreateUpdateElectionScopeRequest();

            factory.ElectionScopeRepository.Setup(repository => repository.GetByIdAsync(request.Id))
                .ReturnsAsync((ElectionScope?)null);

            Result<string> result = await factory.Service.UpdateElectionScope(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election scope with id {request.Id} was not found.", result.Error);

            factory.ElectionScopeRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionScope>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionScope_WithDuplicateName_ShouldReturnConflict()
        {
            using ElectionServiceFactory factory = new();

            UpdateElectionScopeRequest request = ElectionScopeTestData.CreateUpdateElectionScopeRequest();
            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope();

            factory.ElectionScopeRepository.Setup(repository => repository.GetByIdAsync(request.Id))
                .ReturnsAsync(electionScope);

            factory.ElectionScopeRepository.Setup(repository => repository.AnyAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionScope, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.UpdateElectionScope(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal($"Election scope with name {request.Name.Trim()} already exists.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map(request, electionScope), Times.Never);
            factory.ElectionScopeRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionScope>()), Times.Never);
        }

        [Fact]
        public async Task ToggleElectionScopeActivation_WithExistingScope_ShouldToggleActivation()
        {
            using ElectionServiceFactory factory = new();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(active: true);

            factory.ElectionScopeRepository.Setup(repository => repository.GetByIdAsync(electionScope.Id))
                .ReturnsAsync(electionScope);

            Result<string> result = await factory.Service.ToggleElectionScopeActivation(electionScope.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.False(electionScope.Active);

            factory.ElectionScopeRepository.Verify(repository => repository.UpdateAsync(electionScope), Times.Once);
        }

        [Fact]
        public async Task ToggleElectionScopeActivation_WithMissingScope_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            int electionScopeId = 100;

            factory.ElectionScopeRepository.Setup(repository => repository.GetByIdAsync(electionScopeId))
                .ReturnsAsync((ElectionScope?)null);

            Result<string> result = await factory.Service.ToggleElectionScopeActivation(electionScopeId);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election scope with id {electionScopeId} was not found.", result.Error);

            factory.ElectionScopeRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionScope>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithValidDepartmentElection_ShouldCreateElection()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department", description: "Applies to elections conducted within a department.");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus(id: request.ElectionStatusId, code: ApplicationConstants.ElectionStatusCodes.Draft,
                name: "Draft");

            Department department = DepartmentTestData.CreateDepartment();
            department.Id = request.DepartmentId!.Value;

            Election election = ElectionTestData.CreateElection(name: request.Name, yearId: request.YearId, electionTypeId: request.ElectionTypeId,
                electionStatusId: request.ElectionStatusId, departmentId: request.DepartmentId);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            factory.DepartmentRepository.Setup(repository => repository.GetByIdAsync(request.DepartmentId.Value))
                .ReturnsAsync(department);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(false);

            factory.Mapper.Setup(mapper => mapper.Map<Election>(request))
                .Returns(election);

            factory.ElectionRepository.Setup(repository => repository.AddAsync(election, It.IsAny<bool>()))
                .ReturnsAsync(election);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal($"Election with name {election.Name} created successfully.", result.Value);

            factory.Mapper.Verify(mapper => mapper.Map<Election>(request), Times.Once);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(election, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task CreateElection_WithRegistrationOpenAndApplicationPeriod_ShouldCreateElection()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(electionStatusId: 2);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department", description: "Applies to elections conducted within a department.");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus(id: request.ElectionStatusId, code: ApplicationConstants.ElectionStatusCodes.RegistrationOpen,
                name: "Registration Open");

            Department department = DepartmentTestData.CreateDepartment();
            department.Id = request.DepartmentId!.Value;

            Election election = ElectionTestData.CreateElection(
                name: request.Name,
                yearId: request.YearId,
                electionTypeId: request.ElectionTypeId,
                electionStatusId: request.ElectionStatusId,
                departmentId: request.DepartmentId);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                    It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            factory.DepartmentRepository.Setup(repository => repository.GetByIdAsync(request.DepartmentId.Value))
                .ReturnsAsync(department);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(false);

            factory.Mapper.Setup(mapper => mapper.Map<Election>(request))
                .Returns(election);

            factory.ElectionRepository.Setup(repository => repository.AddAsync(election, It.IsAny<bool>()))
                .ReturnsAsync(election);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal($"Election with name {election.Name} created successfully.", result.Value);

            factory.ElectionRepository.Verify(repository => repository.AddAsync(election, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task CreateElection_WithRegistrationOpenAndNoApplicationPeriod_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(electionStatusId: 2, applicationStartAt: null, applicationEndAt: null);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department", description: "Applies to elections conducted within a department.");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus(id: request.ElectionStatusId,
                code: ApplicationConstants.ElectionStatusCodes.RegistrationOpen, name: "Registration Open");

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                    It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Registration Open requires ApplicationStartAt and ApplicationEndAt.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Theory]
        [InlineData(ApplicationConstants.ElectionStatusCodes.RegistrationClosed, "Registration Closed")]
        [InlineData(ApplicationConstants.ElectionStatusCodes.VotingOpen, "Voting Open")]
        [InlineData(ApplicationConstants.ElectionStatusCodes.Completed, "Completed")]
        [InlineData(ApplicationConstants.ElectionStatusCodes.Cancelled, "Cancelled")]
        public async Task CreateElection_WithInvalidInitialStatus_ShouldReturnValidationError(string statusCode, string statusName)
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(electionStatusId: 10);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department", description: "Applies to elections conducted within a department.");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus(id: request.ElectionStatusId, code: statusCode,
                name: statusName);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("A new election can only be created with Draft or Registration Open status.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithMissingYear_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync((Year?)null);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Year with id {request.YearId} was not found.", result.Error);

            factory.ElectionTypeRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()), Times.Never);

            factory.ElectionStatusRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<int>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithMissingElectionType_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();

            Year year = YearTestData.CreateYear(request.YearId);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((ElectionType?)null);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election type with id {request.ElectionTypeId} was not found.", result.Error);

            factory.ElectionStatusRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<int>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithMissingElectionStatus_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department", description: "Applies to elections conducted within a department.");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync((ElectionStatus?)null);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election status with id {request.ElectionStatusId} was not found.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithValidUniversityElection_ShouldCreateElection()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: null);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 1, code: ApplicationConstants.ElectionScopeCodes.University,
                name: "University", description: "Applies to elections conducted across the university.");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus(id: request.ElectionStatusId,
                code: ApplicationConstants.ElectionStatusCodes.Draft, name: "Draft");

            Election election = ElectionTestData.CreateElection(name: request.Name, yearId: request.YearId, electionTypeId: request.ElectionTypeId,
                electionStatusId: request.ElectionStatusId, facultyId: null, departmentId: null);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(false);

            factory.Mapper.Setup(mapper => mapper.Map<Election>(request))
                .Returns(election);

            factory.ElectionRepository.Setup(repository => repository.AddAsync(election, It.IsAny<bool>()))
                .ReturnsAsync(election);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal($"Election with name {election.Name} created successfully.", result.Value);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(election, It.IsAny<bool>()), Times.Once);
        }

        [Theory]
        [InlineData(1L, null)]
        [InlineData(null, 1L)]
        public async Task CreateElection_WithUniversityScopeAndFacultyOrDepartment_ShouldReturnValidationError(long? facultyId, long? departmentId)
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: facultyId, departmentId: departmentId);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 1, code: ApplicationConstants.ElectionScopeCodes.University,
                name: "University", description: "Applies to elections conducted across the university.");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus(id: request.ElectionStatusId,
                code: ApplicationConstants.ElectionStatusCodes.Draft, name: "Draft");

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("FacultyId and DepartmentId must not be provided for a university election.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithValidFacultyElection_ShouldCreateElection()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: 1, departmentId: null);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 2, code: ApplicationConstants.ElectionScopeCodes.Faculty,
                name: "Faculty", description: "Applies to elections conducted within a faculty.");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus(id: request.ElectionStatusId,
                code: ApplicationConstants.ElectionStatusCodes.Draft, name: "Draft");

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = request.FacultyId!.Value;

            Election election = ElectionTestData.CreateElection(name: request.Name, yearId: request.YearId, electionTypeId: request.ElectionTypeId,
                electionStatusId: request.ElectionStatusId, facultyId: request.FacultyId, departmentId: null);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(request.FacultyId.Value))
                .ReturnsAsync(faculty);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(false);

            factory.Mapper.Setup(mapper => mapper.Map<Election>(request))
                .Returns(election);

            factory.ElectionRepository.Setup(repository => repository.AddAsync(election, It.IsAny<bool>()))
                .ReturnsAsync(election);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal($"Election with name {election.Name} created successfully.", result.Value);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(request.FacultyId.Value), Times.Once);
            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(election, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task CreateElection_WithFacultyScopeAndNoFaculty_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: null);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 2, code: ApplicationConstants.ElectionScopeCodes.Faculty,
                name: "Faculty");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("FacultyId is required for a faculty election.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithFacultyScopeAndDepartment_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: 1, departmentId: 1);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 2, code: ApplicationConstants.ElectionScopeCodes.Faculty,
                name: "Faculty");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("DepartmentId must not be provided for a faculty election.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithMissingFaculty_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: 100, departmentId: null);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 2, code: ApplicationConstants.ElectionScopeCodes.Faculty,
                name: "Faculty");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(request.FacultyId!.Value))
                .ReturnsAsync((Faculty?)null);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Faculty with id {request.FacultyId.Value} was not found.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithDepartmentScopeAndNoDepartment_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: null);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("DepartmentId is required for a department election.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithDepartmentScopeAndFaculty_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: 1, departmentId: 1);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("FacultyId must not be provided for a department election.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithMissingDepartment_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: 100);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            factory.DepartmentRepository.Setup(repository => repository.GetByIdAsync(request.DepartmentId!.Value))
                .ReturnsAsync((Department?)null);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Department with id {request.DepartmentId.Value} was not found.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithDuplicateUniversityElection_ShouldReturnConflict()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: null);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 1, code: ApplicationConstants.ElectionScopeCodes.University,
                name: "University");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("An election already exists for the selected election type, year and scope.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithDuplicateFacultyElection_ShouldReturnConflict()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: 1, departmentId: null);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 2, code: ApplicationConstants.ElectionScopeCodes.Faculty,
                name: "Faculty");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = request.FacultyId!.Value;

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(request.FacultyId.Value))
                .ReturnsAsync(faculty);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("An election already exists for the selected election type, year and scope.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithDuplicateDepartmentElection_ShouldReturnConflict()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: 1);

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            Department department = DepartmentTestData.CreateDepartment();
            department.Id = request.DepartmentId!.Value;

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            factory.DepartmentRepository.Setup(repository => repository.GetByIdAsync(request.DepartmentId.Value))
                .ReturnsAsync(department);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("An election already exists for the selected election type, year and scope.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);
            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithMissingElectionScope_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId);

            electionType.ElectionScope = null!;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("The selected election type does not have a valid election scope.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);

            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateElection_WithUnsupportedElectionScope_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();

            Year year = YearTestData.CreateYear(request.YearId);

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 10, code: "UNSUPPORTED", name: "Unsupported");

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);

            electionType.ElectionScope = electionScope;

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus();

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(electionStatus);

            Result<string> result = await factory.Service.CreateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal($"Election scope {electionScope.Code} is not supported.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<Election>(It.IsAny<CreateElectionRequest>()), Times.Never);

            factory.ElectionRepository.Verify(repository => repository.AddAsync(It.IsAny<Election>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task GetElections_WithFacultyFilter_ShouldReturnFacultyAndDepartmentElections()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new()
            {
                FacultyId = 1
            };

            Year year = YearTestData.CreateYear();

            ElectionStatus electionStatus = ElectionStatusTestData.CreateElectionStatus(code: ApplicationConstants.ElectionStatusCodes.Draft);

            ElectionScope universityScope = ElectionScopeTestData.CreateElectionScope(id: 1, code: ApplicationConstants.ElectionScopeCodes.University,
                name: "University");

            ElectionScope facultyScope = ElectionScopeTestData.CreateElectionScope(id: 2, code: ApplicationConstants.ElectionScopeCodes.Faculty, name: "Faculty");

            ElectionScope departmentScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department,
                name: "Department");

            ElectionType universityElectionType = ElectionTypeTestData.CreateElectionType(id: 1, name: "University Election",
                electionScopeId: universityScope.Id);

            ElectionType facultyElectionType = ElectionTypeTestData.CreateElectionType(id: 2, name: "Faculty Election",
                electionScopeId: facultyScope.Id);

            ElectionType departmentElectionType = ElectionTypeTestData.CreateElectionType(id: 3, name: "Department Election",
                electionScopeId: departmentScope.Id);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            Department department = DepartmentTestData.CreateDepartment(name: "Computer Engineering", facultyId: faculty.Id);

            department.Id = 1;

            Election universityElection = ElectionTestData.CreateElection(name: "University Election 2026", yearId: year.Id,
                electionTypeId: universityElectionType.Id, electionStatusId: electionStatus.Id, facultyId: null, departmentId: null);

            Election facultyElection = ElectionTestData.CreateElection(name: "Engineering Faculty Election 2026", yearId: year.Id,
                electionTypeId: facultyElectionType.Id, electionStatusId: electionStatus.Id, facultyId: faculty.Id, departmentId: null);

            Election departmentElection = ElectionTestData.CreateElection(name: "Computer Engineering Election 2026", yearId: year.Id,
                electionTypeId: departmentElectionType.Id, electionStatusId: electionStatus.Id, facultyId: null,
                departmentId: department.Id);

            await factory.DbContextFactory.Context.Set<Year>().AddAsync(year);
            await factory.DbContextFactory.Context.Set<ElectionStatus>().AddAsync(electionStatus);
            await factory.DbContextFactory.Context.Set<ElectionScope>().AddRangeAsync(universityScope, facultyScope, departmentScope);
            await factory.DbContextFactory.Context.Set<ElectionType>().AddRangeAsync(universityElectionType, facultyElectionType, departmentElectionType);
            await factory.DbContextFactory.Context.Set<Faculty>().AddAsync(faculty);
            await factory.DbContextFactory.Context.Set<Department>().AddAsync(department);
            await factory.DbContextFactory.Context.Set<Election>().AddRangeAsync(universityElection, facultyElection, departmentElection);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.DbContextFactory.Context.ChangeTracker.Clear();

            factory.Mapper.Setup(mapper => mapper.Map<IEnumerable<ElectionResponse>>(It.IsAny<List<Election>>()))
                .Returns((List<Election> elections) => elections.Select(election => new ElectionResponse
                {
                    Id = election.Id,
                    Name = election.Name,
                    YearId = election.YearId,
                    ElectionTypeId = election.ElectionTypeId,
                    ElectionStatusId = election.ElectionStatusId,
                    FacultyId = election.FacultyId ?? (election.Department != null ? election.Department.FacultyId : null),
                    DepartmentId = election.DepartmentId,
                    Active = election.Active
                }));

            Result<IEnumerable<ElectionResponse>> result = await factory.Service.GetElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            List<ElectionResponse> elections = result.Value.ToList();

            Assert.Equal(2, elections.Count);

            Assert.Contains(elections, election => election.Name == facultyElection.Name);
            Assert.Contains(elections, election => election.Name == departmentElection.Name);
            Assert.DoesNotContain(elections, election => election.Name == universityElection.Name);

            Assert.All(elections, election => Assert.Equal(faculty.Id, election.FacultyId));
        }

        [Fact]
        public async Task GetElections_WithActiveFilter_ShouldReturnOnlyActiveElections()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new()
            {
                Active = true
            };

            await SeedElectionsForFiltering(factory);
            SetupElectionListMapper(factory);

            Result<IEnumerable<ElectionResponse>> result = await factory.Service.GetElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            List<ElectionResponse> elections = result.Value.ToList();

            Assert.Equal(3, elections.Count);
            Assert.All(elections, election => Assert.True(election.Active));
            Assert.DoesNotContain(elections, election => election.Name == "Science Faculty Election 2027");
        }

        [Fact]
        public async Task GetElections_WithSearchTerm_ShouldReturnMatchingElections()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new()
            {
                SearchTerm = "Registration Open"
            };

            await SeedElectionsForFiltering(factory);
            SetupElectionListMapper(factory);

            Result<IEnumerable<ElectionResponse>> result = await factory.Service.GetElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            List<ElectionResponse> elections = result.Value.ToList();

            Assert.Equal(2, elections.Count);
            Assert.All(elections, election => Assert.Equal("Registration Open", election.ElectionStatus));

            Assert.Contains(elections, election => election.Name == "Engineering Faculty Election 2026");
            Assert.Contains(elections, election => election.Name == "Computer Engineering Election 2026");
        }

        [Fact]
        public async Task GetElections_WithYearFilter_ShouldReturnMatchingElections()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new()
            {
                YearId = 2
            };

            await SeedElectionsForFiltering(factory);
            SetupElectionListMapper(factory);

            Result<IEnumerable<ElectionResponse>> result = await factory.Service.GetElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            List<ElectionResponse> elections = result.Value.ToList();

            Assert.Single(elections);

            ElectionResponse election = elections.Single();

            Assert.Equal(2, election.YearId);
            Assert.Equal("2027/2028", election.Year);
            Assert.Equal("Science Faculty Election 2027", election.Name);
        }

        [Fact]
        public async Task GetElections_WithElectionTypeFilter_ShouldReturnMatchingElections()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new()
            {
                ElectionTypeId = 2
            };

            await SeedElectionsForFiltering(factory);
            SetupElectionListMapper(factory);

            Result<IEnumerable<ElectionResponse>> result = await factory.Service.GetElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            List<ElectionResponse> elections = result.Value.ToList();

            Assert.Equal(2, elections.Count);
            Assert.All(elections, election => Assert.Equal(2, election.ElectionTypeId));
            Assert.All(elections, election => Assert.Equal("Faculty Election", election.ElectionType));

            Assert.Contains(elections, election => election.Name == "Engineering Faculty Election 2026");
            Assert.Contains(elections, election => election.Name == "Science Faculty Election 2027");
        }

        [Fact]
        public async Task GetElections_WithElectionStatusFilter_ShouldReturnMatchingElections()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new()
            {
                ElectionStatusId = 2
            };

            await SeedElectionsForFiltering(factory);
            SetupElectionListMapper(factory);

            Result<IEnumerable<ElectionResponse>> result = await factory.Service.GetElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            List<ElectionResponse> elections = result.Value.ToList();

            Assert.Equal(2, elections.Count);
            Assert.All(elections, election => Assert.Equal(2, election.ElectionStatusId));
            Assert.All(elections, election => Assert.Equal("Registration Open", election.ElectionStatus));

            Assert.Contains(elections, election => election.Name == "Engineering Faculty Election 2026");
            Assert.Contains(elections, election => election.Name == "Computer Engineering Election 2026");
        }

        [Fact]
        public async Task GetElections_WithDepartmentFilter_ShouldReturnMatchingElections()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new()
            {
                DepartmentId = 1
            };

            await SeedElectionsForFiltering(factory);
            SetupElectionListMapper(factory);

            Result<IEnumerable<ElectionResponse>> result = await factory.Service.GetElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            List<ElectionResponse> elections = result.Value.ToList();

            Assert.Single(elections);

            ElectionResponse election = elections.Single();

            Assert.Equal(1, election.DepartmentId);
            Assert.Equal("Computer Engineering", election.Department);
            Assert.Equal("Computer Engineering Election 2026", election.Name);
        }

        [Fact]
        public async Task GetElections_WithoutFilters_ShouldReturnAllElections()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new();

            await SeedElectionsForFiltering(factory);
            SetupElectionListMapper(factory);

            Result<IEnumerable<ElectionResponse>> result = await factory.Service.GetElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);

            List<ElectionResponse> elections = result.Value.ToList();

            Assert.Equal(4, elections.Count);

            Assert.Contains(elections, election => election.Name == "University Election 2026");
            Assert.Contains(elections, election => election.Name == "Engineering Faculty Election 2026");
            Assert.Contains(elections, election => election.Name == "Computer Engineering Election 2026");
            Assert.Contains(elections, election => election.Name == "Science Faculty Election 2027");
        }

        [Fact]
        public async Task GetPagedElections_WithoutFilters_ShouldReturnPagedResponse()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new()
            {
                PageNumber = 1,
                PageSize = 2
            };

            await SeedElectionsForFiltering(factory);

            factory.Mapper.Setup(mapper => mapper.Map<PagedResponse<ElectionResponse>>(It.IsAny<PagedList<Election>>()))
                .Returns((PagedList<Election> elections) => new PagedResponse<ElectionResponse>
                {
                    Items = elections.Select(election => new ElectionResponse
                    {
                        Id = election.Id,
                        Name = election.Name,
                        YearId = election.YearId,
                        ElectionTypeId = election.ElectionTypeId,
                        ElectionStatusId = election.ElectionStatusId,
                        FacultyId = election.FacultyId ?? (election.Department != null ? election.Department.FacultyId : null),
                        DepartmentId = election.DepartmentId,
                        Active = election.Active
                    }).ToList(),
                    MetaData = elections.MetaData
                });

            Result<PagedResponse<ElectionResponse>> result = await factory.Service.GetPagedElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);

            Assert.Equal(2, result.Value.Items.Count());
            Assert.Equal(4, result.Value.MetaData.TotalCount);
        }

        [Fact]
        public async Task GetPagedElections_WithFilter_ShouldReturnFilteredPagedResponse()
        {
            using ElectionServiceFactory factory = new();

            ElectionRequest request = new()
            {
                PageNumber = 1,
                PageSize = 1,
                ElectionStatusId = 2
            };

            await SeedElectionsForFiltering(factory);

            factory.Mapper.Setup(mapper => mapper.Map<PagedResponse<ElectionResponse>>(It.IsAny<PagedList<Election>>()))
                .Returns((PagedList<Election> elections) => new PagedResponse<ElectionResponse>
                {
                    Items = elections.Select(election => new ElectionResponse
                    {
                        Id = election.Id,
                        Name = election.Name,
                        YearId = election.YearId,
                        ElectionTypeId = election.ElectionTypeId,
                        ElectionStatusId = election.ElectionStatusId,
                        FacultyId = election.FacultyId ?? (election.Department != null ? election.Department.FacultyId : null),
                        DepartmentId = election.DepartmentId,
                        Active = election.Active
                    }).ToList(),
                    MetaData = elections.MetaData
                });

            Result<PagedResponse<ElectionResponse>> result = await factory.Service.GetPagedElections(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Items);

            Assert.Single(result.Value.Items);
            Assert.Equal(2, result.Value.MetaData.TotalCount);
            Assert.All(result.Value.Items, election => Assert.Equal(2, election.ElectionStatusId));
        }

        [Fact]
        public async Task GetElection_WithExistingElection_ShouldReturnElection()
        {
            using ElectionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection(name: "Engineering Faculty Election 2026", facultyId: 1, departmentId: null);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(election);

            ElectionResponse response = new()
            {
                Id = election.Id,
                Name = election.Name,
                YearId = election.YearId,
                ElectionTypeId = election.ElectionTypeId,
                ElectionStatusId = election.ElectionStatusId,
                FacultyId = election.FacultyId,
                DepartmentId = election.DepartmentId,
                Active = election.Active
            };

            factory.Mapper.Setup(mapper => mapper.Map<ElectionResponse>(election))
                .Returns(response);

            Result<ElectionResponse> result = await factory.Service.GetElection(election.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(election.Id, result.Value.Id);
            Assert.Equal(election.Name, result.Value.Name);
            Assert.Equal(election.FacultyId, result.Value.FacultyId);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionResponse>(election), Times.Once);
        }

        [Fact]
        public async Task GetElection_WithMissingElection_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            string electionId = Guid.NewGuid().ToString();

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Election?)null);

            Result<ElectionResponse> result = await factory.Service.GetElection(electionId);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election with id {electionId} was not found.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionResponse>(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithMissingElection_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();
            request.Id = Guid.NewGuid().ToString();

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Election?)null);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election with id {request.Id} was not found.", result.Error);
        }

        [Fact]
        public async Task UpdateElection_WithoutId_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();
            request.Id = null;

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Election id is required.", result.Error);
        }

        [Fact]
        public async Task UpdateElection_WithMissingYear_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus(id: request.ElectionStatusId,
                code: ApplicationConstants.ElectionStatusCodes.Draft, name: "Draft");

            Election election = ElectionTestData.CreateElection(yearId: request.YearId, electionTypeId: request.ElectionTypeId,
                electionStatusId: request.ElectionStatusId, facultyId: request.FacultyId, departmentId: request.DepartmentId);

            election.Id = request.Id;
            election.ElectionStatus = currentStatus;

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(election);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync((Year?)null);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Year with id {request.YearId} was not found.", result.Error);

            factory.YearRepository.Verify(repository => repository.GetByIdAsync(request.YearId), Times.Once);
        }

        [Fact]
        public async Task UpdateElection_WithMissingElectionType_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus(
                id: request.ElectionStatusId,
                code: ApplicationConstants.ElectionStatusCodes.Draft,
                name: "Draft");

            Election election = ElectionTestData.CreateElection(
                yearId: request.YearId,
                electionTypeId: request.ElectionTypeId,
                electionStatusId: request.ElectionStatusId,
                facultyId: request.FacultyId,
                departmentId: request.DepartmentId);

            election.Id = request.Id;
            election.ElectionStatus = currentStatus;

            Year year = YearTestData.CreateYear(id: request.YearId);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                    It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(election);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((ElectionType?)null);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election type with id {request.ElectionTypeId} was not found.", result.Error);

            factory.ElectionTypeRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task UpdateElection_WithMissingRequestedElectionStatus_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(electionStatusId: 2);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus(id: 1, code: ApplicationConstants.ElectionStatusCodes.Draft,
                name: "Draft");

            Election election = ElectionTestData.CreateElection(yearId: request.YearId, electionTypeId: request.ElectionTypeId,
                electionStatusId: 1, facultyId: request.FacultyId, departmentId: request.DepartmentId);

            election.Id = request.Id;
            election.ElectionStatus = currentStatus;

            Year year = YearTestData.CreateYear(id: request.YearId);

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(election);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync((ElectionStatus?)null);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election status with id {request.ElectionStatusId} was not found.", result.Error);

            factory.ElectionStatusRepository.Verify(repository => repository.GetByIdAsync(request.ElectionStatusId), Times.Once);
        }

        [Fact]
        public async Task UpdateElection_WithInvalidStatusTransition_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(electionStatusId: 5);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus(id: 1, code: ApplicationConstants.ElectionStatusCodes.Draft,
                name: "Draft");

            ElectionStatus requestedStatus = ElectionStatusTestData.CreateElectionStatus(id: 5, code: ApplicationConstants.ElectionStatusCodes.Completed,
                name: "Completed");

            Election election = ElectionTestData.CreateElection(yearId: request.YearId, electionTypeId: request.ElectionTypeId,
                electionStatusId: currentStatus.Id, facultyId: request.FacultyId, departmentId: request.DepartmentId);

            election.Id = request.Id;
            election.ElectionStatus = currentStatus;

            Year year = YearTestData.CreateYear(id: request.YearId);

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(election);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(requestedStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Election status cannot change from Draft to Completed.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithValidStatusTransitionButMissingRequiredPeriod_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(electionStatusId: 2, applicationStartAt: null,
                applicationEndAt: null);

            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus(id: 1, code: ApplicationConstants.ElectionStatusCodes.Draft,
                name: "Draft");

            ElectionStatus requestedStatus = ElectionStatusTestData.CreateElectionStatus(id: 2, code: ApplicationConstants.ElectionStatusCodes.RegistrationOpen,
                name: "Registration Open");

            Election election = ElectionTestData.CreateElection(yearId: request.YearId, electionTypeId: request.ElectionTypeId,
                electionStatusId: currentStatus.Id, facultyId: request.FacultyId, departmentId: request.DepartmentId);

            election.Id = request.Id;
            election.ElectionStatus = currentStatus;

            Year year = YearTestData.CreateYear(id: request.YearId);

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId);

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(election);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                .ReturnsAsync(requestedStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Registration Open requires ApplicationStartAt and ApplicationEndAt.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithElectionTypeWithoutScope_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus(id: request.ElectionStatusId,
                code: ApplicationConstants.ElectionStatusCodes.Draft, name: "Draft");

            Election election = ElectionTestData.CreateElection(yearId: request.YearId, electionTypeId: request.ElectionTypeId,
                electionStatusId: request.ElectionStatusId, facultyId: request.FacultyId, departmentId: request.DepartmentId);

            election.Id = request.Id;
            election.ElectionStatus = currentStatus;

            Year year = YearTestData.CreateYear(id: request.YearId);

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId);

            electionType.ElectionScope = null!;

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(election);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Election type does not have a valid election scope.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithRegistrationClosedAndMissingVoterRegistrationPeriod_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(electionStatusId: 3, voterRegistrationStartAt: null, voterRegistrationEndAt: null);

            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus(id: 2, code: ApplicationConstants.ElectionStatusCodes.RegistrationOpen, name: "Registration Open");

            ElectionStatus requestedStatus = ElectionStatusTestData.CreateElectionStatus(id: 3, code: ApplicationConstants.ElectionStatusCodes.RegistrationClosed, name: "Registration Closed");

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department, name: "Department");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus, requestedStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Registration Closed requires the application and voter registration periods.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithVotingOpenAndMissingVotingPeriod_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(electionStatusId: 4, votingStartAt: null, votingEndAt: null);

            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus(id: 3, code: ApplicationConstants.ElectionStatusCodes.RegistrationClosed, name: "Registration Closed");

            ElectionStatus requestedStatus = ElectionStatusTestData.CreateElectionStatus(id: 4, code: ApplicationConstants.ElectionStatusCodes.VotingOpen, name: "Voting Open");

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department, name: "Department");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus, requestedStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Voting Open requires VotingStartAt and VotingEndAt.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithCompletedAndMissingVotingPeriod_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(electionStatusId: 5, votingStartAt: null, votingEndAt: null);

            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus(id: 4, code: ApplicationConstants.ElectionStatusCodes.VotingOpen, name: "Voting Open");

            ElectionStatus requestedStatus = ElectionStatusTestData.CreateElectionStatus(id: 5, code: ApplicationConstants.ElectionStatusCodes.Completed, name: "Completed");

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department, name: "Department");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus, requestedStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Completed requires VotingStartAt and VotingEndAt.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithUniversityScopeAndLocation_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: 1);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 1, code: ApplicationConstants.ElectionScopeCodes.University, name: "University");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("University elections cannot have FacultyId or DepartmentId.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithFacultyScopeWithoutFaculty_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: null);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 2, code: ApplicationConstants.ElectionScopeCodes.Faculty, name: "Faculty");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Faculty elections require FacultyId.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithFacultyScopeAndDepartment_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: 1, departmentId: 1);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 2, code: ApplicationConstants.ElectionScopeCodes.Faculty, name: "Faculty");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Faculty elections cannot have DepartmentId.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithMissingFaculty_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: 1, departmentId: null);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 2, code: ApplicationConstants.ElectionScopeCodes.Faculty, name: "Faculty");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(request.FacultyId.Value))
                .ReturnsAsync((Faculty?)null);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Faculty with id {request.FacultyId.Value} was not found.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithDepartmentScopeWithoutDepartment_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: null);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department, name: "Department");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Department elections require DepartmentId.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithDepartmentScopeAndFaculty_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: 1, departmentId: 1);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department, name: "Department");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Department elections cannot have FacultyId.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithMissingDepartment_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: 1);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3,
                code: ApplicationConstants.ElectionScopeCodes.Department, name: "Department");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            factory.DepartmentRepository.Setup(repository => repository.GetByIdAsync(request.DepartmentId.Value))
                .ReturnsAsync((Department?)null);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Department with id {request.DepartmentId.Value} was not found.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithUnsupportedScope_ShouldReturnValidationError()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: null);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 10,
                code: "UNSUPPORTED", name: "Unsupported");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Election scope UNSUPPORTED is not supported.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithDuplicateElection_ShouldReturnConflict()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(facultyId: null, departmentId: 1);
            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3,
                code: ApplicationConstants.ElectionScopeCodes.Department, name: "Department");

            SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            Department department = DepartmentTestData.CreateDepartment();
            department.Id = request.DepartmentId.Value;

            factory.DepartmentRepository.Setup(repository => repository.GetByIdAsync(request.DepartmentId.Value))
                .ReturnsAsync(department);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("An election already exists for the selected election type, year and scope.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElection_WithValidUniversityElection_ShouldUpdateElection()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(name: "Updated University Election",
                facultyId: null, departmentId: null);

            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 1,
                code: ApplicationConstants.ElectionScopeCodes.University, name: "University");

            Election election = SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(false);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Election updated successfully.", result.Value);

            factory.Mapper.Verify(mapper => mapper.Map(request, election), Times.Once);
            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(election), Times.Once);
        }

        [Fact]
        public async Task UpdateElection_WithValidFacultyElection_ShouldUpdateElection()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(name: "Updated Faculty Election",
                facultyId: 1, departmentId: null);

            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 2,
                code: ApplicationConstants.ElectionScopeCodes.Faculty, name: "Faculty");

            Election election = SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = request.FacultyId.Value;

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(request.FacultyId.Value))
                .ReturnsAsync(faculty);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(false);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Election updated successfully.", result.Value);

            factory.Mapper.Verify(mapper => mapper.Map(request, election), Times.Once);
            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(election), Times.Once);
        }

        [Fact]
        public async Task UpdateElection_WithValidDepartmentElection_ShouldUpdateElectionWithoutReloadingSameStatus()
        {
            using ElectionServiceFactory factory = new();

            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(name: "Updated Department Election", facultyId: null, departmentId: 1);

            request.Id = Guid.NewGuid().ToString();

            ElectionStatus currentStatus = ElectionStatusTestData.CreateElectionStatus();

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: 3, code: ApplicationConstants.ElectionScopeCodes.Department, name: "Department");

            Election election = SetupUpdateElectionDependencies(factory, request, electionScope, currentStatus);

            Department department = DepartmentTestData.CreateDepartment();
            department.Id = request.DepartmentId.Value;

            factory.DepartmentRepository.Setup(repository => repository.GetByIdAsync(request.DepartmentId.Value))
                .ReturnsAsync(department);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>()))
                .ReturnsAsync(false);

            Result<string> result = await factory.Service.UpdateElection(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Election updated successfully.", result.Value);

            factory.ElectionStatusRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<int>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map(request, election), Times.Once);
            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(election), Times.Once);
        }

        [Fact]
        public async Task ToggleElectionActivation_WithMissingElection_ShouldReturnNotFound()
        {
            using ElectionServiceFactory factory = new();

            string electionId = Guid.NewGuid().ToString();

            factory.ElectionRepository.Setup(repository => repository.GetByIdAsync(electionId))
                .ReturnsAsync((Election?)null);

            Result<string> result = await factory.Service.ToggleElectionActivation(electionId);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal($"Election with id {electionId} was not found.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Election>()), Times.Never);
        }

        [Fact]
        public async Task ToggleElectionActivation_WithActiveElection_ShouldDeactivateElection()
        {
            using ElectionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();
            election.Active = true;

            factory.ElectionRepository.Setup(repository => repository.GetByIdAsync(election.Id))
                .ReturnsAsync(election);

            Result<string> result = await factory.Service.ToggleElectionActivation(election.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Election deactivated successfully.", result.Value);
            Assert.False(election.Active);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(election), Times.Once);
        }

        [Fact]
        public async Task ToggleElectionActivation_WithInactiveElection_ShouldActivateElection()
        {
            using ElectionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();
            election.Active = false;

            factory.ElectionRepository.Setup(repository => repository.GetByIdAsync(election.Id))
                .ReturnsAsync(election);

            Result<string> result = await factory.Service.ToggleElectionActivation(election.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Election activated successfully.", result.Value);
            Assert.True(election.Active);

            factory.ElectionRepository.Verify(repository => repository.UpdateAsync(election), Times.Once);
        }

        [Fact]
        public void ElectionPositionCountForElectionResolver_WithElectionPositions_ShouldReturnCount()
        {
            Election election = ElectionTestData.CreateElection();

            election.ElectionPositions.Add(new ElectionPosition
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 1000,
                Currency = "NGN"
            });

            election.ElectionPositions.Add(new ElectionPosition
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 1500,
                Currency = "NGN"
            });

            ElectionPositionCountForElectionResolver resolver = new();

            int result = resolver.Resolve(election, new ElectionResponse(), 0, null!);

            Assert.Equal(2, result);
        }

        [Fact]
        public void ElectionPositionCountForElectionResolver_WithNoElectionPositions_ShouldReturnZero()
        {
            Election election = ElectionTestData.CreateElection();

            ElectionPositionCountForElectionResolver resolver = new();

            int result = resolver.Resolve(election, new ElectionResponse(), 0, null!);

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetElection_WithElectionPositions_ShouldReturnNumberOfElectionPositions()
        {
            using ElectionServiceFactory factory = new();

            Election election = ElectionTestData.CreateElection();

            election.ElectionPositions.Add(new ElectionPosition
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 1000,
                Currency = "NGN"
            });

            election.ElectionPositions.Add(new ElectionPosition
            {
                Id = Guid.NewGuid().ToString(),
                ElectionId = election.Id,
                PositionId = Guid.NewGuid().ToString(),
                ApplicationFee = 1500,
                Currency = "NGN"
            });

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(election);

            ElectionResponse response = new()
            {
                Id = election.Id,
                Name = election.Name,
                YearId = election.YearId,
                ElectionTypeId = election.ElectionTypeId,
                ElectionStatusId = election.ElectionStatusId,
                FacultyId = election.FacultyId,
                DepartmentId = election.DepartmentId,
                Active = election.Active,
                NumberOfElectionPositions = election.ElectionPositions.Count
            };

            factory.Mapper.Setup(mapper => mapper.Map<ElectionResponse>(election))
                .Returns(response);

            Result<ElectionResponse> result = await factory.Service.GetElection(election.Id);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.NumberOfElectionPositions);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionResponse>(election), Times.Once);
        }

        private static async Task SeedElectionsForFiltering(ElectionServiceFactory factory)
        {
            List<Year> years = new()
            {
                YearTestData.CreateYear(1, "2026/2027"),
                YearTestData.CreateYear(2, "2027/2028")
            };

            List<ElectionStatus> electionStatuses = new()
            {
                ElectionStatusTestData.CreateElectionStatus(1, ApplicationConstants.ElectionStatusCodes.Draft, "Draft"),

                ElectionStatusTestData.CreateElectionStatus(2, ApplicationConstants.ElectionStatusCodes.RegistrationOpen, "Registration Open"),

                ElectionStatusTestData.CreateElectionStatus(5, ApplicationConstants.ElectionStatusCodes.Completed, "Completed")
            };

            List<ElectionScope> electionScopes = new()
            {
                ElectionScopeTestData.CreateElectionScope(1, ApplicationConstants.ElectionScopeCodes.University, "University"),

                ElectionScopeTestData.CreateElectionScope(2, ApplicationConstants.ElectionScopeCodes.Faculty, "Faculty"),

                ElectionScopeTestData.CreateElectionScope(3, ApplicationConstants.ElectionScopeCodes.Department, "Department")
            };

            List<ElectionType> electionTypes = new()
            {
                ElectionTypeTestData.CreateElectionType(id: 1, name: "University Election", electionScopeId: 1),

                ElectionTypeTestData.CreateElectionType(id: 2, name: "Faculty Election", electionScopeId: 2),

                ElectionTypeTestData.CreateElectionType(id: 3, name: "Department Election", electionScopeId: 3)
            };

            Faculty engineeringFaculty = FacultyTestData.CreateFaculty("Engineering");
            engineeringFaculty.Id = 1;

            Faculty scienceFaculty = FacultyTestData.CreateFaculty("Science");
            scienceFaculty.Id = 2;

            Department department = DepartmentTestData.CreateDepartment(name: "Computer Engineering", facultyId: engineeringFaculty.Id);

            department.Id = 1;

            await factory.DbContextFactory.Context.Set<Year>().AddRangeAsync(years);
            await factory.DbContextFactory.Context.Set<ElectionStatus>().AddRangeAsync(electionStatuses);
            await factory.DbContextFactory.Context.Set<ElectionScope>().AddRangeAsync(electionScopes);
            await factory.DbContextFactory.Context.Set<Faculty>().AddRangeAsync(engineeringFaculty, scienceFaculty);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            await factory.DbContextFactory.Context.Set<ElectionType>().AddRangeAsync(electionTypes);
            await factory.DbContextFactory.Context.Set<Department>().AddAsync(department);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            List<Election> elections = ElectionTestData.CreateElectionsForFiltering();

            await factory.DbContextFactory.Context.Set<Election>().AddRangeAsync(elections);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.DbContextFactory.Context.ChangeTracker.Clear();
        }

        private static void SetupElectionListMapper(ElectionServiceFactory factory)
        {
            factory.Mapper.Setup(mapper => mapper.Map<IEnumerable<ElectionResponse>>(It.IsAny<List<Election>>()))
                .Returns((List<Election> elections) => elections.Select(election => new ElectionResponse
                {
                    Id = election.Id,
                    Name = election.Name,
                    YearId = election.YearId,
                    Year = election.Year.Name,
                    ElectionTypeId = election.ElectionTypeId,
                    ElectionType = election.ElectionType.Name,
                    ElectionStatusId = election.ElectionStatusId,
                    ElectionStatus = election.ElectionStatus.Name,
                    FacultyId = election.FacultyId ?? (election.Department != null ? election.Department.FacultyId : null),
                    Faculty = election.Faculty != null ? election.Faculty.Name
                        : election.Department != null ? election.Department.Faculty.Name : null,
                    DepartmentId = election.DepartmentId,
                    Department = election.Department?.Name,
                    Active = election.Active
                }));
        }

        private static Election SetupUpdateElectionDependencies(ElectionServiceFactory factory, CreateElectionRequest request,
            ElectionScope electionScope, ElectionStatus currentStatus, ElectionStatus? requestedStatus = null)
        {
            Election election = ElectionTestData.CreateElection(yearId: request.YearId, electionTypeId: request.ElectionTypeId,
                electionStatusId: currentStatus.Id, facultyId: request.FacultyId, departmentId: request.DepartmentId);

            election.Id = request.Id!;
            election.ElectionStatus = currentStatus;

            Year year = YearTestData.CreateYear(id: request.YearId);

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(id: request.ElectionTypeId, electionScopeId: electionScope.Id);
            electionType.ElectionScope = electionScope;

            factory.ElectionRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Election, bool>>>(),
                It.IsAny<Func<IQueryable<Election>, IOrderedQueryable<Election>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Election>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Election, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(election);

            factory.YearRepository.Setup(repository => repository.GetByIdAsync(request.YearId))
                .ReturnsAsync(year);

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ElectionType, bool>>>(),
                It.IsAny<Func<IQueryable<ElectionType>, IOrderedQueryable<ElectionType>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<ElectionType>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ElectionType, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(electionType);

            if (requestedStatus is not null && requestedStatus.Id != currentStatus.Id)
            {
                factory.ElectionStatusRepository.Setup(repository => repository.GetByIdAsync(request.ElectionStatusId))
                    .ReturnsAsync(requestedStatus);
            }

            return election;
        }
    }
}