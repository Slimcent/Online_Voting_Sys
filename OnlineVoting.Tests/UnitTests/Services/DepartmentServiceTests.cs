using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Caching.Keys;
using OnlineVoting.Services.Caching.Policies;
using OnlineVoting.Services.Caching.Tags;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;
using System.Linq.Expressions;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class DepartmentServiceTests
    {
        [Fact]
        public async Task CreateDepartment_WithNoNameOrNames_ShouldReturnValidationError()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1
            };

            Result<string> result = await factory.Service.CreateDepartment(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Provide either a department name or a list of department names.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<object>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.AddAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateDepartment_WithNameAndNames_ShouldReturnValidationError()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Name = "Computer Engineering",
                Names = new List<string> { "Electrical Engineering" }
            };

            Result<string> result = await factory.Service.CreateDepartment(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Provide either Name or Names, but not both.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<object>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.AddAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Department>>()), Times.Never);
        }

        [Fact]
        public async Task CreateDepartment_WithMissingFaculty_ShouldReturnNotFound()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Name = "Computer Engineering"
            };

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Faculty?)null);

            Result<string> result = await factory.Service.CreateDepartment(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Faculty with id 1 was not found.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<Department, bool>>>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.AddAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateDepartment_WithExistingSingleDepartment_ShouldReturnConflict()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Name = " Computer Engineering "
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            Department existingDepartment = DepartmentTestData.CreateDepartment("Computer Engineering");
            existingDepartment.Id = 1;

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.DepartmentRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<Department, bool>>>()))
                .ReturnsAsync(existingDepartment);

            Result<string> result = await factory.Service.CreateDepartment(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Department with name Computer Engineering already exists.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.AddAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateDepartment_WithValidSingleDepartment_ShouldCreateDepartment()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Name = " Computer Engineering "
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering");

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.DepartmentRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<Department, bool>>>()))
                .ReturnsAsync((Department?)null);

            factory.Mapper.Setup(mapper => mapper.Map<Department>(request))
                .Returns(department);

            factory.DepartmentRepository.Setup(repository => repository.AddAsync(department, false))
                .ReturnsAsync(department);

            Result<string> result = await factory.Service.CreateDepartment(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("Department with name Computer Engineering created successfully", result.Value);
            Assert.Equal("Computer Engineering", department.Name);

            factory.Mapper.Verify(mapper => mapper.Map<Department>(request), Times.Once);
            factory.DepartmentRepository.Verify(repository => repository.AddAsync(department, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateDepartment_WithEmptyNamesList_ShouldReturnValidationError()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Names = new List<string> { "", " " }
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            Result<string> result = await factory.Service.CreateDepartment(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Department names cannot be empty.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Department>>()), Times.Never);
        }

        [Fact]
        public async Task CreateDepartment_WhenAllMultipleDepartmentsExist_ShouldReturnSuccessWithoutAdding()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Names = new List<string>
                {
                    "Computer Engineering",
                    "Electrical Engineering"
                }
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.DepartmentRepository.Setup(repository => repository.SelectAsync(It.IsAny<Expression<Func<Department, bool>>>(), It.IsAny<Expression<Func<Department, string>>>()))
                .ReturnsAsync(new List<string>
                {
                    "Computer Engineering",
                    "Electrical Engineering"
                });

            Result<string> result = await factory.Service.CreateDepartment(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("No new departments were created.", result.Value);

            factory.DepartmentRepository.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Department>>()), Times.Never);
        }

        [Fact]
        public async Task CreateDepartment_WithMultipleDepartments_ShouldSkipExistingAndCreateNewDepartments()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Names = new List<string>
                {
                    "Computer Engineering",
                    "Electrical Engineering",
                    "Mechanical Engineering",
                    "computer engineering"
                }
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            List<Department> mappedDepartments = new()
            {
                DepartmentTestData.CreateDepartment("Electrical Engineering", 0),
                DepartmentTestData.CreateDepartment("Mechanical Engineering", 0)
            };

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.DepartmentRepository.Setup(repository => repository.SelectAsync(It.IsAny<Expression<Func<Department, bool>>>(), It.IsAny<Expression<Func<Department, string>>>()))
                .ReturnsAsync(new List<string> { "Computer Engineering" });

            factory.Mapper.Setup(mapper => mapper.Map<List<Department>>(It.Is<List<string>>(names => names.Count == 2 
                && names.Contains("Electrical Engineering") 
                && names.Contains("Mechanical Engineering"))))
                .Returns(mappedDepartments);

            Result<string> result = await factory.Service.CreateDepartment(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("2 departments created successfully.", result.Value);
            Assert.All(mappedDepartments, department => Assert.Equal(1, department.FacultyId));

            factory.DepartmentRepository.Verify(repository => repository.AddRangeAsync(mappedDepartments), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetDepartment_WithExistingDepartment_ShouldReturnSuccess()
        {
            DepartmentServiceFactory factory = new();

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering");
            department.Id = 1;

            DepartmentResponse response = DepartmentTestData.CreateDepartmentResponse(1, "Computer Engineering", 1);

            factory.DepartmentRepository
                .Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<Expression<Func<Department, bool>>>(),
                    null,
                    null,
                    null,
                    It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>(),
                    false))
                .ReturnsAsync(department);

            factory.Mapper
                .Setup(mapper => mapper.Map<DepartmentResponse>(department))
                .Returns(response);

            Result<DepartmentResponse> result = await factory.Service.GetDepartment(1L);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("Computer Engineering", result.Value.Name);

            factory.Mapper.Verify(mapper => mapper.Map<DepartmentResponse>(department), Times.Once);
        }

        [Fact]
        public async Task GetDepartment_WithMissingDepartment_ShouldReturnNotFound()
        {
            DepartmentServiceFactory factory = new();

            factory.DepartmentRepository
                .Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<Expression<Func<Department, bool>>>(),
                    null,
                    null,
                    null,
                    It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>(),
                    false))
                .ReturnsAsync((Department?)null);

            Result<DepartmentResponse> result = await factory.Service.GetDepartment(1L);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Department with id 1 was not found.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<DepartmentResponse>(It.IsAny<Department>()), Times.Never);
        }

        [Fact]
        public async Task GetDepartments_WithoutSearchTerm_ShouldReturnPagedResponse()
        {
            DepartmentServiceFactory factory = new();

            DepartmentRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            List<Department> departments = DepartmentTestData.CreateDepartments();

            PagedList<Department> pagedDepartments = new(departments, departments.Count, 1, 10);

            PagedResponse<DepartmentResponse> mappedResponse = new()
            {
                MetaData = pagedDepartments.MetaData,
                Items = departments.Select(department =>
                    DepartmentTestData.CreateDepartmentResponse(department.Id, department.Name, department.FacultyId, department.Active))
            };

            factory.DepartmentRepository
                .Setup(repository => repository.GetPagedItems(
                    request,
                    null,
                    It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>()))
                .ReturnsAsync(pagedDepartments);

            factory.Mapper
                .Setup(mapper => mapper.Map<PagedResponse<DepartmentResponse>>(pagedDepartments))
                .Returns(mappedResponse);

            Result<PagedResponse<DepartmentResponse>> result = await factory.Service.GetDepartments(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value.MetaData!.TotalCount);
            Assert.Equal(3, result.Value.Items!.Count());
        }

        [Fact]
        public async Task GetDepartments_WithSearchTerm_ShouldReturnFilteredPagedResponse()
        {
            DepartmentServiceFactory factory = new();

            DepartmentRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Computer"
            };

            List<Department> departments = new()
    {
        DepartmentTestData.CreateDepartment("Computer Engineering")
    };

            PagedList<Department> pagedDepartments = new(departments, departments.Count, 1, 10);

            PagedResponse<DepartmentResponse> mappedResponse = new()
            {
                MetaData = pagedDepartments.MetaData,
                Items = new List<DepartmentResponse>
        {
            DepartmentTestData.CreateDepartmentResponse(1, "Computer Engineering", 1)
        }
            };

            factory.DepartmentRepository
                .Setup(repository => repository.GetPagedItems(
                    request,
                    It.IsAny<Expression<Func<Department, bool>>>(),
                    It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>()))
                .ReturnsAsync(pagedDepartments);

            factory.Mapper
                .Setup(mapper => mapper.Map<PagedResponse<DepartmentResponse>>(pagedDepartments))
                .Returns(mappedResponse);

            Result<PagedResponse<DepartmentResponse>> result = await factory.Service.GetDepartments(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items!);
            Assert.Equal("Computer Engineering", result.Value.Items!.First().Name);
        }

        [Fact]
        public async Task GetDepartmentsByFacultyId_WithMissingFaculty_ShouldReturnNotFound()
        {
            DepartmentServiceFactory factory = new();

            factory.FacultyRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Faculty?)null);

            Result<IEnumerable<DepartmentResponse>> result = await factory.Service.GetDepartmentsByFacultyId(1L);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Faculty with id 1 was not found.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.GetByAsync(
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IOrderedQueryable<Department>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>(),
                It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task GetDepartmentsByFacultyId_WithExistingFaculty_ShouldReturnDepartments()
        {
            DepartmentServiceFactory factory = new();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            List<Department> departments = DepartmentTestData.CreateDepartments(1);

            List<DepartmentResponse> responses = departments
                .Select(department => DepartmentTestData.CreateDepartmentResponse(department.Id, department.Name, department.FacultyId, department.Active))
                .ToList();

            factory.FacultyRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.DepartmentRepository
                .Setup(repository => repository.GetByAsync(
                    It.IsAny<Expression<Func<Department, bool>>>(),
                    It.IsAny<Func<IQueryable<Department>, IOrderedQueryable<Department>>>(),
                    null,
                    null,
                    It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>(),
                    false))
                .ReturnsAsync(departments);

            factory.Mapper
                .Setup(mapper => mapper.Map<IEnumerable<DepartmentResponse>>(departments))
                .Returns(responses);

            Result<IEnumerable<DepartmentResponse>> result = await factory.Service.GetDepartmentsByFacultyId(1L);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value.Count());
        }

        [Fact]
        public async Task GetDepartmentsByFacultyIdPaged_WithMissingFaculty_ShouldReturnNotFound()
        {
            DepartmentServiceFactory factory = new();

            DepartmentRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            factory.FacultyRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Faculty?)null);

            Result<PagedResponse<DepartmentResponse>> result = await factory.Service.GetDepartmentsByFacultyId(1L, request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Faculty with id 1 was not found.", result.Error);
        }

        [Fact]
        public async Task GetDepartmentsByFacultyIdPaged_WithoutSearchTerm_ShouldReturnPagedDepartments()
        {
            DepartmentServiceFactory factory = new();

            DepartmentRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            List<Department> departments = DepartmentTestData.CreateDepartments(1);

            PagedList<Department> pagedDepartments = new(departments, departments.Count, 1, 10);

            PagedResponse<DepartmentResponse> mappedResponse = new()
            {
                MetaData = pagedDepartments.MetaData,
                Items = departments.Select(department =>
                    DepartmentTestData.CreateDepartmentResponse(department.Id, department.Name, department.FacultyId, department.Active))
            };

            factory.FacultyRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.DepartmentRepository
                .Setup(repository => repository.GetPagedItems(
                    request,
                    It.IsAny<Expression<Func<Department, bool>>>(),
                    It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>()))
                .ReturnsAsync(pagedDepartments);

            factory.Mapper
                .Setup(mapper => mapper.Map<PagedResponse<DepartmentResponse>>(pagedDepartments))
                .Returns(mappedResponse);

            Result<PagedResponse<DepartmentResponse>> result = await factory.Service.GetDepartmentsByFacultyId(1L, request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value.MetaData!.TotalCount);
            Assert.Equal(3, result.Value.Items!.Count());
        }

        [Fact]
        public async Task UpdateDepartment_WithEmptyName_ShouldReturnValidationError()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Name = " "
            };

            Result<string> result = await factory.Service.UpdateDepartment(1L, request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Department name cannot be empty.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<object>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDepartment_WithMissingDepartment_ShouldReturnNotFound()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Name = "Computer Engineering"
            };

            factory.DepartmentRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Department?)null);

            Result<string> result = await factory.Service.UpdateDepartment(1L, request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Department with id 1 was not found.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<object>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDepartment_WithMissingFaculty_ShouldReturnNotFound()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 2,
                Name = "Computer Engineering"
            };

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering", 1);
            department.Id = 1;

            factory.DepartmentRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(department);

            factory.FacultyRepository
                .Setup(repository => repository.GetByIdAsync(2L))
                .ReturnsAsync((Faculty?)null);

            Result<string> result = await factory.Service.UpdateDepartment(1L, request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Faculty with id 2 was not found.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Department, bool>>>()), Times.Never);
            factory.DepartmentRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDepartment_WithDuplicateName_ShouldReturnConflict()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 1,
                Name = " Electrical Engineering "
            };

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering", 1);
            department.Id = 1;

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            factory.DepartmentRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(department);

            factory.FacultyRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.DepartmentRepository
                .Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Department, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.UpdateDepartment(1L, request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Department with name Electrical Engineering already exists.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDepartment_WithValidRequest_ShouldUpdateDepartment()
        {
            DepartmentServiceFactory factory = new();

            CreateDepartmentRequest request = new()
            {
                FacultyId = 2,
                Name = " Updated Computer Engineering "
            };

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering", 1);
            department.Id = 1;

            Faculty faculty = FacultyTestData.CreateFaculty("Science");
            faculty.Id = 2;

            factory.DepartmentRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(department);

            factory.FacultyRepository
                .Setup(repository => repository.GetByIdAsync(2L))
                .ReturnsAsync(faculty);

            factory.DepartmentRepository
                .Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Department, bool>>>()))
                .ReturnsAsync(false);

            factory.DepartmentRepository
                .Setup(repository => repository.UpdateAsync(department, false))
                .ReturnsAsync(department);

            Result<string> result = await factory.Service.UpdateDepartment(1L, request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Department updated successfully.", result.Value);
            Assert.Equal("Updated Computer Engineering", department.Name);
            Assert.Equal(2, department.FacultyId);

            factory.DepartmentRepository.Verify(repository => repository.UpdateAsync(department, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleDepartmentActivation_WithMissingDepartment_ShouldReturnNotFound()
        {
            DepartmentServiceFactory factory = new();

            factory.DepartmentRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Department?)null);

            Result<string> result = await factory.Service.ToggleDepartmentActivation(1L);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Department with id 1 was not found.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task ToggleDepartmentActivation_WithActiveDepartment_ShouldDeactivateDepartment()
        {
            DepartmentServiceFactory factory = new();

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering", 1, true);
            department.Id = 1;

            factory.DepartmentRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(department);

            factory.DepartmentRepository
                .Setup(repository => repository.UpdateAsync(department, false))
                .ReturnsAsync(department);

            Result<string> result = await factory.Service.ToggleDepartmentActivation(1L);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Department deactivated successfully.", result.Value);
            Assert.False(department.Active);

            factory.DepartmentRepository.Verify(repository => repository.UpdateAsync(department, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleDepartmentActivation_WithInactiveDepartment_ShouldActivateDepartment()
        {
            DepartmentServiceFactory factory = new();

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering", 1, false);
            department.Id = 1;

            factory.DepartmentRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(department);

            factory.DepartmentRepository
                .Setup(repository => repository.UpdateAsync(department, false))
                .ReturnsAsync(department);

            Result<string> result = await factory.Service.ToggleDepartmentActivation(1L);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Department activated successfully.", result.Value);
            Assert.True(department.Active);

            factory.DepartmentRepository.Verify(repository => repository.UpdateAsync(department, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteDepartment_WithMissingDepartment_ShouldReturnNotFound()
        {
            DepartmentServiceFactory factory = new();

            factory.DepartmentRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Department?)null);

            Result<string> result = await factory.Service.DeleteDepartment(1L);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Department with id 1 was not found.", result.Error);

            factory.DepartmentRepository.Verify(repository => repository.DeleteAsync(It.IsAny<Department>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task DeleteDepartment_WithExistingDepartment_ShouldDeleteDepartment()
        {
            DepartmentServiceFactory factory = new();

            Department department = DepartmentTestData.CreateDepartment("Computer Engineering");
            department.Id = 1;

            factory.DepartmentRepository
                .Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(department);

            factory.DepartmentRepository
                .Setup(repository => repository.DeleteAsync(department, false))
                .Returns(Task.CompletedTask);

            Result<string> result = await factory.Service.DeleteDepartment(1L);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Department deleted successfully.", result.Value);

            factory.DepartmentRepository.Verify(repository => repository.DeleteAsync(department, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetDepartment_WithCachedDepartment_ShouldReturnCachedDepartmentWithoutQueryingRepository()
        {
            DepartmentServiceFactory factory = new();

            DepartmentResponse cachedResponse = new()
            {
                Id = 1,
                Name = "Computer Engineering"
            };

            factory.CacheService.Setup(cacheService => cacheService.GetOrCreate<DepartmentResponse?>(DepartmentCacheKeys.GetDepartment(1),
                It.IsAny<Func<CancellationToken, ValueTask<DepartmentResponse?>>>(),
                CachePolicies.Department,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<DepartmentResponse?>(cachedResponse));

            Result<DepartmentResponse> result = await factory.Service.GetDepartment(1);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("Computer Engineering", result.Value.Name);

            factory.DepartmentRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IOrderedQueryable<Department>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>(),
                It.IsAny<bool>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<DepartmentResponse>(It.IsAny<Department>()), Times.Never);
        }

        [Fact]
        public async Task GetDepartments_WithCachedDepartments_ShouldReturnCachedResponseWithoutQueryingRepository()
        {
            DepartmentServiceFactory factory = new();

            DepartmentRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<DepartmentResponse> cachedResponse = new()
            {
                Items = new List<DepartmentResponse>
                {
                    new DepartmentResponse
                    {
                        Id = 1,
                        Name = "Computer Engineering"
                    }
                },
                MetaData = new MetaData
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalCount = 1,
                    TotalPages = 1
                }
            };

            factory.CacheService.Setup(cacheService => cacheService.GetOrCreate<PagedResponse<DepartmentResponse>>(DepartmentCacheKeys.GetDepartments(request),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<DepartmentResponse>>>>(),
                CachePolicies.Department,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(cachedResponse));

            Result<PagedResponse<DepartmentResponse>> result = await factory.Service.GetDepartments(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items!);
            Assert.Equal("Computer Engineering", result.Value.Items!.First().Name);

            factory.DepartmentRepository.Verify(repository => repository.GetPagedItems(It.IsAny<DepartmentRequestParameters>(),
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<PagedResponse<DepartmentResponse>>(It.IsAny<PagedList<Department>>()), Times.Never);
        }

        [Fact]
        public async Task GetDepartmentsByFacultyId_WithCachedDepartments_ShouldReturnCachedDepartmentsWithoutQueryingRepositories()
        {
            DepartmentServiceFactory factory = new();

            IEnumerable<DepartmentResponse> cachedResponse = new List<DepartmentResponse>
            {
                new DepartmentResponse
                {
                    Id = 1,
                    Name = "Computer Engineering"
                }
            };

            factory.CacheService.Setup(cacheService => cacheService.GetOrCreate<IEnumerable<DepartmentResponse>>(DepartmentCacheKeys.GetDepartmentsByFacultyId(1),
                It.IsAny<Func<CancellationToken, ValueTask<IEnumerable<DepartmentResponse>>>>(),
                CachePolicies.Department,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(cachedResponse));

            Result<IEnumerable<DepartmentResponse>> result = await factory.Service.GetDepartmentsByFacultyId(1);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.Equal("Computer Engineering", result.Value.First().Name);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);

            factory.DepartmentRepository.Verify(repository => repository.GetByAsync(It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IOrderedQueryable<Department>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>(),
                It.IsAny<bool>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<IEnumerable<DepartmentResponse>>(It.IsAny<IEnumerable<Department>>()), Times.Never);
        }

        [Fact]
        public async Task GetDepartmentsByFacultyId_WithCachedPagedDepartments_ShouldReturnCachedResponseWithoutQueryingRepositories()
        {
            DepartmentServiceFactory factory = new();

            DepartmentRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<DepartmentResponse> cachedResponse = new()
            {
                Items = new List<DepartmentResponse>
                {
                    new DepartmentResponse
                    {
                        Id = 1,
                        Name = "Computer Engineering"
                    }
                },
                MetaData = new MetaData
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalCount = 1,
                    TotalPages = 1
                }
            };

            factory.CacheService.Setup(cacheService => cacheService.GetOrCreate<PagedResponse<DepartmentResponse>>(DepartmentCacheKeys.GetDepartmentsByFacultyId(1, request),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<DepartmentResponse>>>>(),
                CachePolicies.Department,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(cachedResponse));

            Result<PagedResponse<DepartmentResponse>> result = await factory.Service.GetDepartmentsByFacultyId(1, request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items!);
            Assert.Equal("Computer Engineering", result.Value.Items!.First().Name);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);

            factory.DepartmentRepository.Verify(repository => repository.GetPagedItems(It.IsAny<DepartmentRequestParameters>(),
                It.IsAny<Expression<Func<Department, bool>>>(),
                It.IsAny<Func<IQueryable<Department>, IIncludableQueryable<Department, object>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<PagedResponse<DepartmentResponse>>(It.IsAny<PagedList<Department>>()), Times.Never);
        }
    }
}