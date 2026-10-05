using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineVoting.Api.Controllers;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Tests.TestData.Data;

namespace OnlineVoting.Tests.UnitTests.Api.Controllers
{
    public class ElectionControllerTests
    {
        private readonly Mock<IElectionService> _electionService;
        private readonly ElectionController _controller;

        public ElectionControllerTests()
        {
            _electionService = new Mock<IElectionService>();
            _controller = new ElectionController(_electionService.Object);
        }

        [Fact]
        public async Task GetElectionStatuses_ShouldReturnOk()
        {
            List<ElectionStatusResponse> response = new()
            {
                new ElectionStatusResponse
                {
                    Id = 1,
                    Name = "Draft",
                    Active = true,
                    NumberOfElections = 2
                }
            };

            _electionService.Setup(service => service.GetElectionStatuses())
                .ReturnsAsync(Result<IEnumerable<ElectionStatusResponse>>.Success(response));

            IActionResult result = await _controller.GetElectionStatuses();

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);

            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            IEnumerable<ElectionStatusResponse> value = Assert.IsAssignableFrom<IEnumerable<ElectionStatusResponse>>(successResponse.Data);

            Assert.True(successResponse.Success);
            Assert.Single(value);

            _electionService.Verify(service => service.GetElectionStatuses(), Times.Once);
        }

        [Fact]
        public async Task GetPagedElectionStatuses_ShouldReturnOk()
        {
            ElectionStatusRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<ElectionStatusResponse> response = new()
            {
                Items = new List<ElectionStatusResponse>
                {
                    new ElectionStatusResponse
                    {
                        Id = 1,
                        Name = "Draft",
                        Active = true,
                        NumberOfElections = 2
                    }
                }
            };

            _electionService.Setup(service => service.GetPagedElectionStatuses(request))
                .ReturnsAsync(Result<PagedResponse<ElectionStatusResponse>>.Success(response));

            IActionResult result = await _controller.GetPagedElectionStatuses(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);

            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            PagedResponse<ElectionStatusResponse> value = Assert.IsType<PagedResponse<ElectionStatusResponse>>(successResponse.Data);

            Assert.True(successResponse.Success);
            Assert.NotNull(value.Items);
            Assert.Single(value.Items);

            _electionService.Verify(service => service.GetPagedElectionStatuses(request), Times.Once);
        }

        [Fact]
        public async Task GetElectionStatus_WithExistingStatus_ShouldReturnOk()
        {
            ElectionStatusResponse response = new()
            {
                Id = 1,
                Name = "Draft",
                Description = "The election is still being configured.",
                Active = true,
                NumberOfElections = 2
            };

            _electionService.Setup(service => service.GetElectionStatus(1))
                .ReturnsAsync(Result<ElectionStatusResponse>.Success(response));

            IActionResult result = await _controller.GetElectionStatus(1);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);

            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            ElectionStatusResponse value = Assert.IsType<ElectionStatusResponse>(successResponse.Data);

            Assert.True(successResponse.Success);
            Assert.Equal(1, value.Id);
            Assert.Equal("Draft", value.Name);
            Assert.Equal(2, value.NumberOfElections);

            _electionService.Verify(service => service.GetElectionStatus(1), Times.Once);
        }

        [Fact]
        public async Task UpdateElectionStatus_WithValidRequest_ShouldReturnOk()
        {
            UpdateElectionStatusRequest request = ElectionStatusTestData.CreateUpdateElectionStatusRequest();

            _electionService.Setup(service => service.UpdateElectionStatus(request))
                .ReturnsAsync(Result<string>.Success("Election status updated successfully."));

            IActionResult result = await _controller.UpdateElectionStatus(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);

            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            string value = Assert.IsType<string>(successResponse.Data);

            Assert.True(successResponse.Success);
            Assert.Equal("Election status updated successfully.", value);

            _electionService.Verify(service => service.UpdateElectionStatus(request), Times.Once);
        }

        [Fact]
        public async Task CreateElection_WithValidRequest_ShouldReturnCreated()
        {
            CreateElectionRequest request = ElectionTestData.CreateElectionRequest();

            string response = $"Election with name {request.Name} created successfully.";

            _electionService.Setup(service => service.CreateElection(request))
                .ReturnsAsync(Result<string>.Created(response));

            IActionResult result = await _controller.CreateElection(request);

            ObjectResult objectResult = Assert.IsAssignableFrom<ObjectResult>(result);

            Assert.Equal(201, objectResult.StatusCode);

            string value = Assert.IsType<string>(objectResult.Value);

            Assert.Equal(response, value);

            _electionService.Verify(service => service.CreateElection(request), Times.Once);
        }

        [Fact]
        public async Task GetElections_WithFilters_ShouldReturnOk()
        {
            ElectionRequest request = new()
            {
                YearId = 1,
                ElectionStatusId = 2,
                Active = true
            };

            List<ElectionResponse> response = new()
            {
                new ElectionResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Engineering Faculty Election 2026",
                    YearId = 1,
                    Year = "2026/2027",
                    ElectionTypeId = 2,
                    ElectionType = "Faculty Election",
                    ElectionStatusId = 2,
                    ElectionStatus = "Registration Open",
                    FacultyId = 1,
                    Faculty = "Engineering",
                    Active = true
                }
            };

            _electionService.Setup(service => service.GetElections(request))
                .ReturnsAsync(Result<IEnumerable<ElectionResponse>>.Success(response));

            IActionResult result = await _controller.GetElections(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            IEnumerable<ElectionResponse> value = Assert.IsAssignableFrom<IEnumerable<ElectionResponse>>(successResponse.Data);

            Assert.True(successResponse.Success);
            Assert.Single(value);

            ElectionResponse election = value.Single();

            Assert.Equal("Engineering Faculty Election 2026", election.Name);
            Assert.Equal(1, election.YearId);
            Assert.Equal(2, election.ElectionStatusId);
            Assert.True(election.Active);

            _electionService.Verify(service => service.GetElections(request), Times.Once);
        }

        [Fact]
        public async Task GetPagedElections_WithFilters_ShouldReturnOk()
        {
            ElectionRequest request = new()
            {
                PageNumber = 1,
                PageSize = 2,
                ElectionStatusId = 2,
                Active = true
            };

            PagedResponse<ElectionResponse> response = new()
            {
                Items = new List<ElectionResponse>
                {
                    new ElectionResponse
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Engineering Faculty Election 2026",
                        YearId = 1,
                        Year = "2026/2027",
                        ElectionTypeId = 2,
                        ElectionType = "Faculty Election",
                        ElectionStatusId = 2,
                        ElectionStatus = "Registration Open",
                        FacultyId = 1,
                        Faculty = "Engineering",
                        Active = true
                    }
                }
            };

            _electionService.Setup(service => service.GetPagedElections(request))
                .ReturnsAsync(Result<PagedResponse<ElectionResponse>>.Success(response));

            IActionResult result = await _controller.GetPagedElections(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            PagedResponse<ElectionResponse> value = Assert.IsType<PagedResponse<ElectionResponse>>(successResponse.Data);

            Assert.True(successResponse.Success);
            Assert.NotNull(value.Items);
            Assert.Single(value.Items);

            ElectionResponse election = value.Items.Single();

            Assert.Equal("Engineering Faculty Election 2026", election.Name);
            Assert.Equal(2, election.ElectionStatusId);
            Assert.True(election.Active);

            _electionService.Verify(service => service.GetPagedElections(request), Times.Once);
        }

        [Fact]
        public async Task GetElection_WithExistingElection_ShouldReturnOk()
        {
            string electionId = Guid.NewGuid().ToString();

            ElectionResponse response = new()
            {
                Id = electionId,
                Name = "Engineering Faculty Election 2026",
                YearId = 1,
                Year = "2026/2027",
                ElectionTypeId = 2,
                ElectionType = "Faculty Election",
                ElectionStatusId = 2,
                ElectionStatus = "Registration Open",
                FacultyId = 1,
                Faculty = "Engineering",
                Active = true
            };

            _electionService.Setup(service => service.GetElection(electionId))
                .ReturnsAsync(Result<ElectionResponse>.Success(response));

            IActionResult result = await _controller.GetElection(electionId);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            ElectionResponse value = Assert.IsType<ElectionResponse>(successResponse.Data);

            Assert.True(successResponse.Success);
            Assert.Equal(electionId, value.Id);
            Assert.Equal("Engineering Faculty Election 2026", value.Name);
            Assert.Equal(2, value.ElectionStatusId);

            _electionService.Verify(service => service.GetElection(electionId), Times.Once);
        }

        [Fact]
        public async Task UpdateElection_WithValidRequest_ShouldReturnOk()
        {
            CreateElectionRequest request = ElectionTestData.CreateElectionRequest(name: "Updated Department Election", electionStatusId: 2,
                facultyId: null, departmentId: 1);

            request.Id = Guid.NewGuid().ToString();

            _electionService.Setup(service => service.UpdateElection(request))
                .ReturnsAsync(Result<string>.Success("Election updated successfully."));

            IActionResult result = await _controller.UpdateElection(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            string value = Assert.IsType<string>(successResponse.Data);

            Assert.True(successResponse.Success);
            Assert.Equal("Election updated successfully.", value);

            _electionService.Verify(service => service.UpdateElection(request), Times.Once);
        }

        [Fact]
        public async Task ToggleElectionActivation_WithExistingElection_ShouldReturnOk()
        {
            string electionId = Guid.NewGuid().ToString();

            _electionService.Setup(service => service.ToggleElectionActivation(electionId))
                .ReturnsAsync(Result<string>.Success("Election deactivated successfully."));

            IActionResult result = await _controller.ToggleElectionActivation(electionId);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            string value = Assert.IsType<string>(successResponse.Data);

            Assert.True(successResponse.Success);
            Assert.Equal("Election deactivated successfully.", value);

            _electionService.Verify(service => service.ToggleElectionActivation(electionId), Times.Once);
        }
    }
}