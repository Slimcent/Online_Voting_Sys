using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Entities.OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;
using System.Linq.Expressions;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class ElectionTypeServiceTests
    {
        [Fact]
        public async Task CreateElectionType_WithValidRequest_ShouldReturnCreated()
        {
            using ElectionTypeServiceFactory factory = new();

            CreateElectionTypeRequest request = ElectionTypeTestData.CreateElectionTypeRequest();

            ElectionType electionType = ElectionTypeTestData.CreateElectionType();

            factory.ElectionTypeRepository.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<ElectionType, bool>>>()))
                .ReturnsAsync(false);

            factory.Mapper.Setup(mapper => mapper.Map<ElectionType>(request))
                .Returns(electionType);

            Result<string> result = await factory.Service.CreateElectionType(request);

            Assert.Equal(ResultStatus.Created, result.Status);

            factory.ElectionTypeRepository.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<ElectionType, bool>>>()), Times.Once);
            factory.Mapper.Verify(mapper => mapper.Map<ElectionType>(request), Times.Once);
            factory.ElectionTypeRepository.Verify(repository => repository.AddAsync(electionType), Times.Once);
        }

        [Fact]
        public async Task CreateElectionType_WithExistingName_ShouldReturnConflict()
        {
            using ElectionTypeServiceFactory factory = new();

            CreateElectionTypeRequest request = ElectionTypeTestData.CreateElectionTypeRequest();

            factory.ElectionTypeRepository.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<ElectionType, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.CreateElectionType(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Election type with name Department Election already exists.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionType>(It.IsAny<CreateElectionTypeRequest>()), Times.Never);
            factory.ElectionTypeRepository.Verify(repository => repository.AddAsync(It.IsAny<ElectionType>()), Times.Never);
        }

        [Fact]
        public async Task GetElectionTypes_WithExistingElectionTypes_ShouldReturnSuccess()
        {
            using ElectionTypeServiceFactory factory = new();

            ElectionType firstElectionType = ElectionTypeTestData.CreateElectionType(1, "Department Election");
            ElectionType secondElectionType = ElectionTypeTestData.CreateElectionType(2, "Faculty Election");

            await factory.DbContextFactory.Context.Set<ElectionType>().AddRangeAsync(firstElectionType, secondElectionType);
            await factory.DbContextFactory.Context.SaveChangesAsync();

            List<ElectionTypeResponse> responses = new()
            {
                new ElectionTypeResponse
                {
                    Id = 1,
                    Name = "Department Election",
                    ElectionScopeId = 3,
                    ElectionScopeCode = "DEPARTMENT",
                    ElectionScope = "Department",
                    Active = true
                },
                new ElectionTypeResponse
                {
                    Id = 2,
                    Name = "Faculty Election",
                    ElectionScopeId = 2,
                    ElectionScopeCode = "FACULTY",
                    ElectionScope = "Faculty",
                    Active = true
                }
            };

            factory.Mapper.Setup(mapper => mapper.Map<IEnumerable<ElectionTypeResponse>>(It.IsAny<IEnumerable<ElectionType>>()))
                .Returns(responses);

            Result<IEnumerable<ElectionTypeResponse>> result = await factory.Service.GetElectionTypes();

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count());

            factory.Mapper.Verify(mapper => mapper.Map<IEnumerable<ElectionTypeResponse>>(It.IsAny<IEnumerable<ElectionType>>()), Times.Once);
        }

        [Fact]
        public async Task GetElectionTypes_WithNoElectionTypes_ShouldReturnEmptyCollection()
        {
            using ElectionTypeServiceFactory factory = new();

            List<ElectionTypeResponse> responses = new();

            factory.Mapper.Setup(mapper => mapper.Map<IEnumerable<ElectionTypeResponse>>(It.IsAny<IEnumerable<ElectionType>>()))
                .Returns(responses);

            Result<IEnumerable<ElectionTypeResponse>> result = await factory.Service.GetElectionTypes();

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetElectionType_WithExistingElectionType_ShouldReturnSuccess()
        {
            using ElectionTypeServiceFactory factory = new();

            ElectionType electionType = ElectionTypeTestData.CreateElectionType();

            ElectionTypeResponse response = new()
            {
                Id = electionType.Id,
                Name = electionType.Name,
                Description = electionType.Description,
                ElectionScopeId = 3,
                ElectionScopeCode = "DEPARTMENT",
                ElectionScope = "Department",
                Active = electionType.Active
            };

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<ElectionType, bool>>>(),
                null, null, null, It.IsAny<Func<IQueryable<ElectionType>, IIncludableQueryable<ElectionType, object>>>(), false))
                .ReturnsAsync(electionType);

            factory.Mapper.Setup(mapper => mapper.Map<ElectionTypeResponse>(electionType))
                .Returns(response);

            Result<ElectionTypeResponse> result = await factory.Service.GetElectionType(1);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("Department Election", result.Value.Name);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionTypeResponse>(electionType), Times.Once);
        }

        [Fact]
        public async Task GetElectionType_WithMissingElectionType_ShouldReturnNotFound()
        {
            using ElectionTypeServiceFactory factory = new();

            factory.ElectionTypeRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<ElectionType, bool>>>(),
                null, null, null,
                It.IsAny<Func<IQueryable<ElectionType>, IIncludableQueryable<ElectionType, object>>>(), false))
                .ReturnsAsync((ElectionType?)null);

            Result<ElectionTypeResponse> result = await factory.Service.GetElectionType(1);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Election type with id 1 was not found.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<ElectionTypeResponse>(It.IsAny<ElectionType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionType_WithValidRequest_ShouldReturnSuccess()
        {
            using ElectionTypeServiceFactory factory = new();

            CreateElectionTypeRequest request = ElectionTypeTestData.CreateUpdateElectionTypeRequest();

            ElectionType electionType = ElectionTypeTestData.CreateElectionType();

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(electionType);

            factory.ElectionTypeRepository.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<ElectionType, bool>>>()))
                .ReturnsAsync(false);

            factory.Mapper.Setup(mapper => mapper.Map(request, electionType))
                .Returns(electionType);

            Result<string> result = await factory.Service.UpdateElectionType(request);

            Assert.Equal(ResultStatus.Success, result.Status);

            factory.Mapper.Verify(mapper => mapper.Map(request, electionType), Times.Once);
            factory.ElectionTypeRepository.Verify(repository => repository.UpdateAsync(electionType), Times.Once);
        }

        [Fact]
        public async Task UpdateElectionType_WithMissingElectionType_ShouldReturnNotFound()
        {
            using ElectionTypeServiceFactory factory = new();

            CreateElectionTypeRequest request = ElectionTypeTestData.CreateUpdateElectionTypeRequest();

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync((ElectionType?)null);

            Result<string> result = await factory.Service.UpdateElectionType(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Election type with id 1 was not found.", result.Error);

            factory.ElectionTypeRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionType_WithExistingName_ShouldReturnConflict()
        {
            using ElectionTypeServiceFactory factory = new();

            CreateElectionTypeRequest request = ElectionTypeTestData.CreateUpdateElectionTypeRequest();

            ElectionType electionType = ElectionTypeTestData.CreateElectionType();

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(electionType);

            factory.ElectionTypeRepository.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<ElectionType, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.UpdateElectionType(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Election type with name Updated Department Election already exists.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map(It.IsAny<CreateElectionTypeRequest>(), It.IsAny<ElectionType>()), Times.Never);
            factory.ElectionTypeRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionType>()), Times.Never);
        }

        [Fact]
        public async Task ToggleElectionTypeActivation_WithActiveElectionType_ShouldDeactivateElectionType()
        {
            using ElectionTypeServiceFactory factory = new();

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(active: true);

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(electionType);

            Result<string> result = await factory.Service.ToggleElectionTypeActivation(1);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.False(electionType.Active);

            factory.ElectionTypeRepository.Verify(repository => repository.UpdateAsync(electionType), Times.Once);
        }

        [Fact]
        public async Task ToggleElectionTypeActivation_WithInactiveElectionType_ShouldActivateElectionType()
        {
            using ElectionTypeServiceFactory factory = new();

            ElectionType electionType = ElectionTypeTestData.CreateElectionType(active: false);

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(electionType);

            Result<string> result = await factory.Service.ToggleElectionTypeActivation(1);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.True(electionType.Active);

            factory.ElectionTypeRepository.Verify(repository => repository.UpdateAsync(electionType), Times.Once);
        }

        [Fact]
        public async Task ToggleElectionTypeActivation_WithMissingElectionType_ShouldReturnNotFound()
        {
            using ElectionTypeServiceFactory factory = new();

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync((ElectionType?)null);

            Result<string> result = await factory.Service.ToggleElectionTypeActivation(1);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Election type with id 1 was not found.", result.Error);

            factory.ElectionTypeRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionType>()), Times.Never);
        }

        [Fact]
        public async Task DeleteElectionType_WithUnusedElectionType_ShouldReturnSuccess()
        {
            using ElectionTypeServiceFactory factory = new();

            ElectionType electionType = ElectionTypeTestData.CreateElectionType();

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(electionType);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Election, bool>>>()))
                .ReturnsAsync(false);

            Result<string> result = await factory.Service.DeleteElectionType(1);

            Assert.Equal(ResultStatus.Success, result.Status);

            factory.ElectionTypeRepository.Verify(repository => repository.DeleteAsync(electionType), Times.Once);
        }

        [Fact]
        public async Task DeleteElectionType_WithElectionTypeInUse_ShouldReturnConflict()
        {
            using ElectionTypeServiceFactory factory = new();

            ElectionType electionType = ElectionTypeTestData.CreateElectionType();

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(electionType);

            factory.ElectionRepository.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Election, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.DeleteElectionType(1);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Election type cannot be deleted because it is already used by an election.", result.Error);

            factory.ElectionTypeRepository.Verify(repository => repository.DeleteAsync(It.IsAny<ElectionType>()), Times.Never);
        }

        [Fact]
        public async Task DeleteElectionType_WithMissingElectionType_ShouldReturnNotFound()
        {
            using ElectionTypeServiceFactory factory = new();

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync((ElectionType?)null);

            Result<string> result = await factory.Service.DeleteElectionType(1);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Election type with id 1 was not found.", result.Error);

            factory.ElectionRepository.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Election, bool>>>()), Times.Never);
            factory.ElectionTypeRepository.Verify(repository => repository.DeleteAsync(It.IsAny<ElectionType>()), Times.Never);
        }

        [Fact]
        public async Task GetPagedElectionTypes_WithoutSearchTerm_ShouldReturnPagedResponse()
        {
            using ElectionTypeServiceFactory factory = new();

            ElectionTypeRequest request = new()
            {
                PageNumber = 1,
                PageSize = 2
            };

            ElectionType firstElectionType = ElectionTypeTestData.CreateElectionType(1, "Department Election");
            ElectionType secondElectionType = ElectionTypeTestData.CreateElectionType(2, "Faculty Election");
            ElectionType thirdElectionType = ElectionTypeTestData.CreateElectionType(3, "Student Election");

            await factory.DbContextFactory.Context.Set<ElectionType>().AddRangeAsync(firstElectionType, secondElectionType, thirdElectionType);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(mapper => mapper.Map<PagedResponse<ElectionTypeResponse>>(It.IsAny<PagedList<ElectionType>>()))
                .Returns((PagedList<ElectionType> electionTypes) => new PagedResponse<ElectionTypeResponse>
                {
                    Items = electionTypes.Select(electionType => new ElectionTypeResponse
                    {
                        Id = electionType.Id,
                        Name = electionType.Name,
                        Description = electionType.Description,
                        ElectionScopeId = electionType.Name == "Department Election" ? 3
                            : electionType.Name == "Faculty Election" ? 2
                            : 1,
                        ElectionScopeCode = electionType.Name == "Department Election" ? "DEPARTMENT"
                            : electionType.Name == "Faculty Election" ? "FACULTY"
                            : "UNIVERSITY",
                        ElectionScope = electionType.Name == "Department Election" ? "Department"
                            : electionType.Name == "Faculty Election" ? "Faculty"
                            : "University",
                        Active = electionType.Active
                    }).ToList(),
                    MetaData = electionTypes.MetaData
                });

            Result<PagedResponse<ElectionTypeResponse>> result = await factory.Service.GetPagedElectionTypes(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.MetaData);
            Assert.NotNull(result.Value.Items);
            Assert.Equal(3, result.Value.MetaData.TotalCount);
            Assert.Equal(2, result.Value.Items.Count());

            factory.Mapper.Verify(mapper => mapper.Map<PagedResponse<ElectionTypeResponse>>(It.IsAny<PagedList<ElectionType>>()), Times.Once);
        }

        [Fact]
        public async Task GetPagedElectionTypes_WithSearchTerm_ShouldReturnFilteredPagedResponse()
        {
            using ElectionTypeServiceFactory factory = new();

            ElectionTypeRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Faculty"
            };

            ElectionType firstElectionType = ElectionTypeTestData.CreateElectionType(1, "Department Election");
            ElectionType secondElectionType = ElectionTypeTestData.CreateElectionType(2, "Faculty Election");
            ElectionType thirdElectionType = ElectionTypeTestData.CreateElectionType(3, "Student Election");

            await factory.DbContextFactory.Context.Set<ElectionType>().AddRangeAsync(firstElectionType, secondElectionType, thirdElectionType);

            await factory.DbContextFactory.Context.SaveChangesAsync();

            factory.Mapper.Setup(mapper => mapper.Map<PagedResponse<ElectionTypeResponse>>(It.IsAny<PagedList<ElectionType>>()))
                .Returns((PagedList<ElectionType> electionTypes) => new PagedResponse<ElectionTypeResponse>
                {
                    Items = electionTypes.Select(electionType => new ElectionTypeResponse
                    {
                        Id = electionType.Id,
                        Name = electionType.Name,
                        Description = electionType.Description,
                        ElectionScopeId = 2,
                        ElectionScopeCode = "FACULTY",
                        ElectionScope = "Faculty",
                        Active = electionType.Active
                    }).ToList(),
                    MetaData = electionTypes.MetaData
                });

            Result<PagedResponse<ElectionTypeResponse>> result = await factory.Service.GetPagedElectionTypes(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.MetaData);
            Assert.NotNull(result.Value.Items);
            Assert.Equal(1, result.Value.MetaData.TotalCount);

            ElectionTypeResponse electionType = Assert.Single(result.Value.Items);

            Assert.Equal("Faculty Election", electionType.Name);
        }

        [Fact]
        public async Task CreateElectionType_WithInactiveScope_ShouldReturnValidationError()
        {
            using ElectionTypeServiceFactory factory = new();

            CreateElectionTypeRequest request = new()
            {
                Name = "Faculty Election",
                Description = "Faculty election.",
                ElectionScopeId = 2
            };

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(
                id: request.ElectionScopeId,
                code: "FACULTY",
                name: "Faculty",
                active: false);

            factory.ElectionScopeRepository.Setup(repository => repository.GetByIdAsync(request.ElectionScopeId))
                .ReturnsAsync(electionScope);

            Result<string> result = await factory.Service.CreateElectionType(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal($"Election scope with id {request.ElectionScopeId} is inactive.", result.Error);

            factory.ElectionTypeRepository.Verify(repository => repository.AddAsync(It.IsAny<ElectionType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionType_WithInactiveDifferentScope_ShouldReturnValidationError()
        {
            using ElectionTypeServiceFactory factory = new();

            CreateElectionTypeRequest request = new()
            {
                Id = 1,
                Name = "Department Election",
                Description = "Updated department election.",
                ElectionScopeId = 2
            };

            ElectionType electionType = new()
            {
                Id = request.Id.Value,
                Name = "Department Election",
                Description = "Department election.",
                ElectionScopeId = 3,
                Active = true
            };

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(
                id: request.ElectionScopeId,
                code: "FACULTY",
                name: "Faculty",
                active: false);

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(request.Id.Value))
                .ReturnsAsync(electionType);

            factory.ElectionScopeRepository.Setup(repository => repository.GetByIdAsync(request.ElectionScopeId))
                .ReturnsAsync(electionScope);

            Result<string> result = await factory.Service.UpdateElectionType(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal($"Election scope with id {request.ElectionScopeId} is inactive.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map(request, electionType), Times.Never);
            factory.ElectionTypeRepository.Verify(repository => repository.UpdateAsync(It.IsAny<ElectionType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateElectionType_WithSameInactiveScope_ShouldReturnSuccess()
        {
            using ElectionTypeServiceFactory factory = new();

            CreateElectionTypeRequest request = new()
            {
                Id = 1,
                Name = "Faculty Election Updated",
                Description = "Updated faculty election.",
                ElectionScopeId = 2
            };

            ElectionType electionType = new()
            {
                Id = request.Id.Value,
                Name = "Faculty Election",
                Description = "Faculty election.",
                ElectionScopeId = 2,
                Active = true
            };

            ElectionScope electionScope = ElectionScopeTestData.CreateElectionScope(id: request.ElectionScopeId, code: "FACULTY", name: "Faculty",
                active: false);

            factory.ElectionTypeRepository.Setup(repository => repository.GetByIdAsync(request.Id.Value))
                .ReturnsAsync(electionType);

            factory.ElectionScopeRepository.Setup(repository => repository.GetByIdAsync(request.ElectionScopeId))
                .ReturnsAsync(electionScope);

            factory.ElectionTypeRepository.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<ElectionType, bool>>>()))
                .ReturnsAsync(false);

            factory.Mapper.Setup(mapper => mapper.Map(request, electionType))
                .Returns(electionType);

            Result<string> result = await factory.Service.UpdateElectionType(request);

            Assert.Equal(ResultStatus.Success, result.Status);

            factory.Mapper.Verify(mapper => mapper.Map(request, electionType), Times.Once);
            factory.ElectionTypeRepository.Verify(repository => repository.UpdateAsync(electionType), Times.Once);
        }
    }
}