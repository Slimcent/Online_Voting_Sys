using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Enums;
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
    public class FacultyServiceTests
    {
        [Fact]
        public async Task CreateFaculty_WithNoNameOrNames_ShouldReturnValidationError()
        {
            FacultyServiceFactory factory = new();

            CreateFacultyRequest request = new();

            Result<string> result = await factory.Service.CreateFaculty(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Provide either a faculty name or a list of faculty names.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<Faculty, bool>>>()), Times.Never);
            factory.FacultyRepository.Verify(repository => repository.AddAsync(It.IsAny<Faculty>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateFaculty_WithNameAndNames_ShouldReturnValidationError()
        {
            FacultyServiceFactory factory = new();

            CreateFacultyRequest request = new()
            {
                Name = "Engineering",
                Names = new List<string> { "Science" }
            };

            Result<string> result = await factory.Service.CreateFaculty(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Provide either Name or Names, but not both.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.AddAsync(It.IsAny<Faculty>(), It.IsAny<bool>()), Times.Never);
            factory.FacultyRepository.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Faculty>>()), Times.Never);
        }

        [Fact]
        public async Task CreateFaculty_WithExistingSingleFaculty_ShouldReturnConflict()
        {
            FacultyServiceFactory factory = new();

            CreateFacultyRequest request = new()
            {
                Name = " Engineering "
            };

            Faculty existingFaculty = FacultyTestData.CreateFaculty("Engineering");

            factory.FacultyRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<Faculty, bool>>>()))
                .ReturnsAsync(existingFaculty);

            Result<string> result = await factory.Service.CreateFaculty(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Faculty with name Engineering already exists.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.AddAsync(It.IsAny<Faculty>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task CreateFaculty_WithValidSingleFaculty_ShouldCreateFaculty()
        {
            FacultyServiceFactory factory = new();

            CreateFacultyRequest request = new()
            {
                Name = " Engineering "
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");

            factory.FacultyRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<Faculty, bool>>>()))
                .ReturnsAsync((Faculty?)null);

            factory.Mapper.Setup(mapper => mapper.Map<Faculty>(request))
                .Returns(faculty);

            factory.FacultyRepository.Setup(repository => repository.AddAsync(faculty, false))
                .ReturnsAsync(faculty);

            Result<string> result = await factory.Service.CreateFaculty(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("Faculty with name Engineering created successfully", result.Value);
            Assert.Equal("Engineering", faculty.Name);

            factory.Mapper.Verify(mapper => mapper.Map<Faculty>(request), Times.Once);
            factory.FacultyRepository.Verify(repository => repository.AddAsync(faculty, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateFaculty_WithEmptyNamesList_ShouldReturnValidationError()
        {
            FacultyServiceFactory factory = new();

            CreateFacultyRequest request = new()
            {
                Names = new List<string> { "", " " }
            };

            Result<string> result = await factory.Service.CreateFaculty(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Faculty names cannot be empty.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Faculty>>()), Times.Never);
        }

        [Fact]
        public async Task CreateFaculty_WhenAllMultipleFacultiesExist_ShouldReturnSuccessWithoutAdding()
        {
            FacultyServiceFactory factory = new();

            CreateFacultyRequest request = new()
            {
                Names = new List<string> { "Engineering", "Science" }
            };

            factory.FacultyRepository.Setup(repository => repository.SelectAsync(It.IsAny<Expression<Func<Faculty, bool>>>(), It.IsAny<Expression<Func<Faculty, string>>>()))
                .ReturnsAsync(new List<string> { "Engineering", "Science" });

            Result<string> result = await factory.Service.CreateFaculty(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("No new faculties were created.", result.Value);

            factory.FacultyRepository.Verify(repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Faculty>>()), Times.Never);
        }

        [Fact]
        public async Task CreateFaculty_WithMultipleFaculties_ShouldSkipExistingAndCreateNewFaculties()
        {
            FacultyServiceFactory factory = new();

            CreateFacultyRequest request = new()
            {
                Names = new List<string> { "Engineering", "Science", "Arts", "engineering" }
            };

            List<Faculty> mappedFaculties = new()
            {
                FacultyTestData.CreateFaculty("Science"),
                FacultyTestData.CreateFaculty("Arts")
            };

            factory.FacultyRepository.Setup(repository => repository.SelectAsync(It.IsAny<Expression<Func<Faculty, bool>>>(), It.IsAny<Expression<Func<Faculty, string>>>()))
                .ReturnsAsync(new List<string> { "Engineering" });

            factory.Mapper.Setup(mapper => mapper.Map<List<Faculty>>(It.Is<List<string>>(names => names.Count == 2 
                && names.Contains("Science") 
                && names.Contains("Arts"))))
                .Returns(mappedFaculties);

            Result<string> result = await factory.Service.CreateFaculty(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.Equal("2 faculties created successfully.", result.Value);

            factory.FacultyRepository.Verify(repository => repository.AddRangeAsync(mappedFaculties), Times.Once);
        }

        [Fact]
        public async Task GetFaculty_WithExistingFaculty_ShouldReturnSuccess()
        {
            FacultyServiceFactory factory = new();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            FacultyResponse response = FacultyTestData.CreateFacultyResponse(1, "Engineering");

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.Mapper.Setup(mapper => mapper.Map<FacultyResponse>(faculty))
                .Returns(response);

            Result<FacultyResponse> result = await factory.Service.GetFaculty(1);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("Engineering", result.Value.Name);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(1L), Times.Once);
            factory.Mapper.Verify(mapper => mapper.Map<FacultyResponse>(faculty), Times.Once);
        }

        [Fact]
        public async Task GetFaculty_WithMissingFaculty_ShouldReturnNotFound()
        {
            FacultyServiceFactory factory = new();

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Faculty?)null);

            Result<FacultyResponse> result = await factory.Service.GetFaculty(1);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Faculty with id 1 was not found.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(1L), Times.Once);
            factory.Mapper.Verify(mapper => mapper.Map<FacultyResponse>(It.IsAny<Faculty>()), Times.Never);
        }

        [Fact]
        public async Task GetFaculties_WithoutSearchTerm_ShouldReturnPagedResponse()
        {
            FacultyServiceFactory factory = new();

            FacultyRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            List<Faculty> faculties = FacultyTestData.CreateFaculties();

            PagedList<Faculty> pagedFaculties = new(faculties, faculties.Count, 1, 10);

            PagedResponse<FacultyResponse> mappedResponse = new()
            {
                MetaData = pagedFaculties.MetaData,
                Items = faculties.Select(faculty => FacultyTestData.CreateFacultyResponse(faculty.Id, faculty.Name, faculty.Active))
            };

            factory.FacultyRepository.Setup(repository => repository.GetPagedItems(request, null, null))
                .ReturnsAsync(pagedFaculties);

            factory.Mapper.Setup(mapper => mapper.Map<PagedResponse<FacultyResponse>>(pagedFaculties))
                .Returns(mappedResponse);

            Result<PagedResponse<FacultyResponse>> result = await factory.Service.GetFaculties(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(3, result.Value.MetaData!.TotalCount);
            Assert.Equal(3, result.Value.Items!.Count());

            factory.FacultyRepository.Verify(repository => repository.GetPagedItems(request, null, null), Times.Once);
        }

        [Fact]
        public async Task GetFaculties_WithSearchTerm_ShouldReturnFilteredPagedResponse()
        {
            FacultyServiceFactory factory = new();

            FacultyRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "Engineering"
            };

            List<Faculty> faculties = new()
            {
                FacultyTestData.CreateFaculty("Engineering")
            };

            PagedList<Faculty> pagedFaculties = new(faculties, faculties.Count, 1, 10);

            PagedResponse<FacultyResponse> mappedResponse = new()
            {
                MetaData = pagedFaculties.MetaData,
                Items = new List<FacultyResponse>
                {
                    FacultyTestData.CreateFacultyResponse(1, "Engineering")
                }
            };

            factory.FacultyRepository.Setup(repository => repository.GetPagedItems(request,
                It.IsAny<Expression<Func<Faculty, bool>>>(), null))
                .ReturnsAsync(pagedFaculties);

            factory.Mapper.Setup(mapper => mapper.Map<PagedResponse<FacultyResponse>>(pagedFaculties))
                .Returns(mappedResponse);

            Result<PagedResponse<FacultyResponse>> result = await factory.Service.GetFaculties(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items!);
            Assert.Equal("Engineering", result.Value.Items!.First().Name);
        }

        [Fact]
        public async Task GetFacultyWithDepartments_WithExistingFaculty_ShouldReturnFacultyWithDepartments()
        {
            FacultyServiceFactory factory = new();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;
            faculty.Departments = new List<Department>
            {
                new Department
                {
                    Id = 1,
                    Name = "Computer Engineering",
                    FacultyId = 1
                },
                new Department
                {
                    Id = 2,
                    Name = "Electrical Engineering",
                    FacultyId = 1
                }
            };

            FacultyResponse response = FacultyTestData.CreateFacultyResponse(1, "Engineering");
            response.Departments = new List<DepartmentResponse>
            {
                new DepartmentResponse
                {
                    Id = 1,
                    Name = "Computer Engineering",
                    FacultyId = 1
                },
                new DepartmentResponse
                {
                    Id = 2,
                    Name = "Electrical Engineering",
                    FacultyId = 1
                }
            };

            factory.FacultyRepository.Setup(repository => repository.GetSingleByAsync(
                    It.IsAny<Expression<Func<Faculty, bool>>>(),
                    null, null, null,
                    It.IsAny<Func<IQueryable<Faculty>, IIncludableQueryable<Faculty, object>>>(),
                    false))
                .ReturnsAsync(faculty);

            factory.Mapper.Setup(mapper => mapper.Map<FacultyResponse>(faculty))
                .Returns(response);

            Result<FacultyResponse> result = await factory.Service.GetFacultyWithDepartments(1);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Departments!.Count());
        }

        [Fact]
        public async Task GetFacultyWithDepartments_WithMissingFaculty_ShouldReturnNotFound()
        {
            FacultyServiceFactory factory = new();

            factory.FacultyRepository.Setup(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<Faculty, bool>>>(),
                null, null, null,
                It.IsAny<Func<IQueryable<Faculty>, IIncludableQueryable<Faculty, object>>>(), false))
                .ReturnsAsync((Faculty?)null);

            Result<FacultyResponse> result = await factory.Service.GetFacultyWithDepartments(1);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Faculty with id 1 was not found.", result.Error);

            factory.Mapper.Verify(mapper => mapper.Map<FacultyResponse>(It.IsAny<Faculty>()), Times.Never);
        }

        [Fact]
        public async Task GetFacultiesWithDepartments_WithoutSearchTerm_ShouldReturnPagedResponse()
        {
            FacultyServiceFactory factory = new();

            FacultyRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;
            faculty.Departments = new List<Department>
            {
                new Department
                {
                    Id = 1,
                    Name = "Computer Engineering",
                    FacultyId = 1
                }
            };

            List<Faculty> faculties = new() { faculty };

            PagedList<Faculty> pagedFaculties = new(faculties, faculties.Count, 1, 10);

            PagedResponse<FacultyResponse> mappedResponse = new()
            {
                MetaData = pagedFaculties.MetaData,
                Items = new List<FacultyResponse>
                {
                    new FacultyResponse
                    {
                        Id = 1,
                        Name = "Engineering",
                        Active = true,
                        Departments = new List<DepartmentResponse>
                        {
                            new DepartmentResponse
                            {
                                Id = 1,
                                Name = "Computer Engineering",
                                FacultyId = 1
                            }
                        }
                    }
                }
            };

            factory.FacultyRepository.Setup(repository => repository.GetPagedItems(request, null,
                It.IsAny<Func<IQueryable<Faculty>, IIncludableQueryable<Faculty, object>>>()))
                .ReturnsAsync(pagedFaculties);

            factory.Mapper.Setup(mapper => mapper.Map<PagedResponse<FacultyResponse>>(pagedFaculties))
                .Returns(mappedResponse);

            Result<PagedResponse<FacultyResponse>> result = await factory.Service.GetFacultiesWithDepartments(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items!);
            Assert.Single(result.Value.Items!.First().Departments!);
        }

        [Fact]
        public async Task UpdateFaculty_WithEmptyName_ShouldReturnValidationError()
        {
            FacultyServiceFactory factory = new();

            CreateWithNameRequest request = new()
            {
                Name = " "
            };

            Result<string> result = await factory.Service.UpdateFaculty(1L, request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Faculty name cannot be empty.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<object>()), Times.Never);
            factory.FacultyRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Faculty>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task UpdateFaculty_WithMissingFaculty_ShouldReturnNotFound()
        {
            FacultyServiceFactory factory = new();

            CreateWithNameRequest request = new()
            {
                Name = "Engineering"
            };

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Faculty?)null);

            Result<string> result = await factory.Service.UpdateFaculty(1L, request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Faculty with id 1 was not found.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.AnyAsync(It.IsAny<Expression<Func<Faculty, bool>>>()), Times.Never);
            factory.FacultyRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Faculty>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task UpdateFaculty_WithDuplicateName_ShouldReturnConflict()
        {
            FacultyServiceFactory factory = new();

            CreateWithNameRequest request = new()
            {
                Name = " Science "
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.FacultyRepository.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Faculty, bool>>>()))
                .ReturnsAsync(true);

            Result<string> result = await factory.Service.UpdateFaculty(1L, request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("Faculty with name Science already exists.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Faculty>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task UpdateFaculty_WithValidRequest_ShouldUpdateFaculty()
        {
            FacultyServiceFactory factory = new();

            CreateWithNameRequest request = new()
            {
                Name = " Updated Engineering "
            };

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.FacultyRepository.Setup(repository => repository.AnyAsync(It.IsAny<Expression<Func<Faculty, bool>>>()))
                .ReturnsAsync(false);

            factory.FacultyRepository
                .Setup(repository => repository.UpdateAsync(faculty, false))
                .ReturnsAsync(faculty);

            Result<string> result = await factory.Service.UpdateFaculty(1L, request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Faculty updated successfully.", result.Value);
            Assert.Equal("Updated Engineering", faculty.Name);

            factory.FacultyRepository.Verify(repository => repository.UpdateAsync(faculty, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleFacultyActivation_WithMissingFaculty_ShouldReturnNotFound()
        {
            FacultyServiceFactory factory = new();

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Faculty?)null);

            Result<string> result = await factory.Service.ToggleFacultyActivation(1L);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Faculty with id 1 was not found.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.UpdateAsync(It.IsAny<Faculty>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task ToggleFacultyActivation_WithActiveFaculty_ShouldDeactivateFaculty()
        {
            FacultyServiceFactory factory = new();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;
            faculty.Active = true;

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.FacultyRepository.Setup(repository => repository.UpdateAsync(faculty, false))
                .ReturnsAsync(faculty);

            Result<string> result = await factory.Service.ToggleFacultyActivation(1L);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Faculty deactivated successfully.", result.Value);
            Assert.False(faculty.Active);

            factory.FacultyRepository.Verify(repository => repository.UpdateAsync(faculty, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleFacultyActivation_WithInactiveFaculty_ShouldActivateFaculty()
        {
            FacultyServiceFactory factory = new();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;
            faculty.Active = false;

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.FacultyRepository.Setup(repository => repository.UpdateAsync(faculty, false))
                .ReturnsAsync(faculty);

            Result<string> result = await factory.Service.ToggleFacultyActivation(1L);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Faculty activated successfully.", result.Value);
            Assert.True(faculty.Active);

            factory.FacultyRepository.Verify(repository => repository.UpdateAsync(faculty, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteFaculty_WithMissingFaculty_ShouldReturnNotFound()
        {
            FacultyServiceFactory factory = new();

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync((Faculty?)null);

            Result<string> result = await factory.Service.DeleteFaculty(1L);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("Faculty with id 1 was not found.", result.Error);

            factory.FacultyRepository.Verify(repository => repository.DeleteAsync(It.IsAny<Faculty>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task DeleteFaculty_WithExistingFaculty_ShouldDeleteFaculty()
        {
            FacultyServiceFactory factory = new();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.FacultyRepository.Setup(repository => repository.DeleteAsync(faculty, false))
                .Returns(Task.CompletedTask);

            Result<string> result = await factory.Service.DeleteFaculty(1L);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("Faculty deleted successfully.", result.Value);

            factory.FacultyRepository.Verify(repository => repository.DeleteAsync(faculty, false), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Department, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetFaculty_WithCachedFaculty_ShouldReturnCachedFacultyWithoutQueryingRepository()
        {
            FacultyServiceFactory factory = new();

            FacultyResponse cachedResponse = FacultyTestData.CreateFacultyResponse(1, "Engineering");

            factory.CacheService.Setup(cacheService => cacheService.GetOrCreate<FacultyResponse?>(
                    FacultyCacheKeys.GetFaculty(1),
                    It.IsAny<Func<CancellationToken, ValueTask<FacultyResponse?>>>(),
                    CachePolicies.Faculty,
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<FacultyResponse?>(cachedResponse));

            Result<FacultyResponse> result = await factory.Service.GetFaculty(1);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("Engineering", result.Value.Name);

            factory.FacultyRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<long>()), Times.Never);
            factory.Mapper.Verify(mapper => mapper.Map<FacultyResponse>(It.IsAny<Faculty>()), Times.Never);

            factory.CacheService.Verify(cacheService => cacheService.GetOrCreate<FacultyResponse?>(FacultyCacheKeys.GetFaculty(1),
                It.IsAny<Func<CancellationToken, ValueTask<FacultyResponse?>>>(),
                CachePolicies.Faculty,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetFaculties_WithCachedFaculties_ShouldReturnCachedResponseWithoutQueryingRepository()
        {
            FacultyServiceFactory factory = new();

            FacultyRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<FacultyResponse> cachedResponse = new()
            {
                Items = new List<FacultyResponse>
                {
                    FacultyTestData.CreateFacultyResponse(1, "Engineering")
                },
                MetaData = new MetaData
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalCount = 1,
                    TotalPages = 1
                }
            };

            factory.CacheService.Setup(cacheService => cacheService.GetOrCreate<PagedResponse<FacultyResponse>>(FacultyCacheKeys.GetFaculties(request),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<FacultyResponse>>>>(),
                CachePolicies.Faculty,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(cachedResponse));

            Result<PagedResponse<FacultyResponse>> result = await factory.Service.GetFaculties(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items!);
            Assert.Equal("Engineering", result.Value.Items!.First().Name);

            factory.FacultyRepository.Verify(repository => repository.GetPagedItems(It.IsAny<FacultyRequestParameters>(), null, null), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<PagedResponse<FacultyResponse>>(It.IsAny<PagedList<Faculty>>()), Times.Never);

            factory.CacheService.Verify(cacheService => cacheService.GetOrCreate<PagedResponse<FacultyResponse>>( FacultyCacheKeys.GetFaculties(request),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<FacultyResponse>>>>(),
                CachePolicies.Faculty,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetFacultyWithDepartments_WithCachedFaculty_ShouldReturnCachedFacultyWithoutQueryingRepository()
        {
            FacultyServiceFactory factory = new();

            FacultyResponse cachedResponse = FacultyTestData.CreateFacultyResponse(1, "Engineering");

            factory.CacheService.Setup(cacheService => cacheService.GetOrCreate<FacultyResponse?>(
                    FacultyCacheKeys.GetFacultyWithDepartments(1),
                    It.IsAny<Func<CancellationToken, ValueTask<FacultyResponse?>>>(),
                    CachePolicies.Faculty,
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.FromResult<FacultyResponse?>(cachedResponse));

            Result<FacultyResponse> result = await factory.Service.GetFacultyWithDepartments(1);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("Engineering", result.Value.Name);

            factory.FacultyRepository.Verify(repository => repository.GetSingleByAsync(It.IsAny<Expression<Func<Faculty, bool>>>(),
                It.IsAny<Func<IQueryable<Faculty>, IOrderedQueryable<Faculty>>>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<Func<IQueryable<Faculty>, IIncludableQueryable<Faculty, object>>>(),
                It.IsAny<bool>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<FacultyResponse>(It.IsAny<Faculty>()), Times.Never);
        }

        [Fact]
        public async Task GetFacultiesWithDepartments_WithCachedFaculties_ShouldReturnCachedResponseWithoutQueryingRepository()
        {
            FacultyServiceFactory factory = new();

            FacultyRequestParameters request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<FacultyResponse> cachedResponse = new()
            {
                Items = new List<FacultyResponse>
                {
                    FacultyTestData.CreateFacultyResponse(1, "Engineering")
                },
                MetaData = new MetaData
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalCount = 1,
                    TotalPages = 1
                }
            };

            factory.CacheService.Setup(cacheService => cacheService.GetOrCreate<PagedResponse<FacultyResponse>>(
                FacultyCacheKeys.GetFacultiesWithDepartments(request),
                It.IsAny<Func<CancellationToken, ValueTask<PagedResponse<FacultyResponse>>>>(),
                CachePolicies.Faculty,
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(cachedResponse));

            Result<PagedResponse<FacultyResponse>> result = await factory.Service.GetFacultiesWithDepartments(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items!);
            Assert.Equal("Engineering", result.Value.Items!.First().Name);

            factory.FacultyRepository.Verify(repository => repository.GetPagedItems(It.IsAny<FacultyRequestParameters>(),
                It.IsAny<Expression<Func<Faculty, bool>>>(),
                It.IsAny<Func<IQueryable<Faculty>, IIncludableQueryable<Faculty, object>>>()), Times.Never);

            factory.Mapper.Verify(mapper => mapper.Map<PagedResponse<FacultyResponse>>(It.IsAny<PagedList<Faculty>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateFaculty_WhenSuccessful_ShouldInvalidateFacultyCache()
        {
            FacultyServiceFactory factory = new();

            Faculty faculty = FacultyTestData.CreateFaculty("Engineering");
            faculty.Id = 1;

            CreateWithNameRequest request = new()
            {
                Name = "Technology"
            };

            factory.FacultyRepository.Setup(repository => repository.GetByIdAsync(1L))
                .ReturnsAsync(faculty);

            factory.FacultyRepository.Setup(repository => repository.AnyAsync(
                It.IsAny<Expression<Func<Faculty, bool>>>()))
            .ReturnsAsync(false);

            Result<string> result = await factory.Service.UpdateFaculty(1, request);

            Assert.Equal(ResultStatus.Success, result.Status);

            factory.CacheService.Verify(cacheService => cacheService.RemoveByTag(CacheTags.Faculty, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}