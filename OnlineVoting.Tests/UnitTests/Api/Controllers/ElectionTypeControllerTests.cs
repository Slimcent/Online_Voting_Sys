using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineVoting.Api.Controllers;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;
using OnlineVoting.Tests.TestData.Data;

namespace OnlineVoting.Tests.UnitTests.Api.Controllers
{
    public class ElectionTypeControllerTests
    {
        [Fact]
        public async Task CreateElectionType_WithValidRequest_ShouldReturnCreated()
        {
            Mock<IElectionTypeService> electionTypeService = new();
            CreateElectionTypeRequest request = ElectionTypeTestData.CreateElectionTypeRequest();

            electionTypeService.Setup(service => service.CreateElectionType(request))
                .ReturnsAsync(Result<string>.Created("Election type created successfully."));

            ElectionTypeController controller = new(electionTypeService.Object);

            IActionResult result = await controller.CreateElectionType(request);

            ObjectResult actionResult = Assert.IsAssignableFrom<ObjectResult>(result);

            Assert.Equal(201, actionResult.StatusCode);

            electionTypeService.Verify(service => service.CreateElectionType(request), Times.Once);
        }

        [Fact]
        public async Task GetElectionTypes_ShouldReturnOk()
        {
            Mock<IElectionTypeService> electionTypeService = new();

            IEnumerable<ElectionTypeResponse> response = new List<ElectionTypeResponse>
            {
                new()
                {
                    Id = 2,
                    Name = "Faculty Election",
                    Description = "Test description",
                    ElectionScopeId = 2,
                    ElectionScopeCode = "FACULTY",
                    ElectionScope = "Faculty",
                    Active = true
                }
            };

            electionTypeService.Setup(service => service.GetElectionTypes())
                .ReturnsAsync(Result<IEnumerable<ElectionTypeResponse>>.Success(response));

            ElectionTypeController controller = new(electionTypeService.Object);

            IActionResult result = await controller.GetElectionTypes();

            ObjectResult actionResult = Assert.IsAssignableFrom<ObjectResult>(result);

            Assert.Equal(200, actionResult.StatusCode);

            electionTypeService.Verify(service => service.GetElectionTypes(), Times.Once);
        }

        [Fact]
        public async Task GetElectionType_WithExistingElectionType_ShouldReturnOk()
        {
            Mock<IElectionTypeService> electionTypeService = new();

            ElectionTypeResponse response = new()
            {
                Id = 1,
                Name = "Department Election",
                ElectionScopeId = 3,
                ElectionScopeCode = "DEPARTMENT",
                ElectionScope = "Department",
                Active = true
            };

            electionTypeService.Setup(service => service.GetElectionType(1))
                .ReturnsAsync(Result<ElectionTypeResponse>.Success(response));

            ElectionTypeController controller = new(electionTypeService.Object);

            IActionResult result = await controller.GetElectionType(1);

            ObjectResult actionResult = Assert.IsAssignableFrom<ObjectResult>(result);

            Assert.Equal(200, actionResult.StatusCode);

            electionTypeService.Verify(service => service.GetElectionType(1), Times.Once);
        }

        [Fact]
        public async Task UpdateElectionType_WithValidRequest_ShouldReturnOk()
        {
            Mock<IElectionTypeService> electionTypeService = new();
            CreateElectionTypeRequest request = ElectionTypeTestData.CreateUpdateElectionTypeRequest();

            electionTypeService.Setup(service => service.UpdateElectionType(request))
                .ReturnsAsync(Result<string>.Success("Election type updated successfully."));

            ElectionTypeController controller = new(electionTypeService.Object);

            IActionResult result = await controller.UpdateElectionType(request);

            ObjectResult actionResult = Assert.IsAssignableFrom<ObjectResult>(result);

            Assert.Equal(200, actionResult.StatusCode);

            electionTypeService.Verify(service => service.UpdateElectionType(request), Times.Once);
        }

        [Fact]
        public async Task ToggleElectionTypeActivation_WithExistingElectionType_ShouldReturnOk()
        {
            Mock<IElectionTypeService> electionTypeService = new();

            electionTypeService.Setup(service => service.ToggleElectionTypeActivation(1))
                .ReturnsAsync(Result<string>.Success("Election type deactivated successfully."));

            ElectionTypeController controller = new(electionTypeService.Object);

            IActionResult result = await controller.ToggleElectionTypeActivation(1);

            ObjectResult actionResult = Assert.IsAssignableFrom<ObjectResult>(result);

            Assert.Equal(200, actionResult.StatusCode);

            electionTypeService.Verify(service => service.ToggleElectionTypeActivation(1), Times.Once);
        }

        [Fact]
        public async Task DeleteElectionType_WithExistingElectionType_ShouldReturnOk()
        {
            Mock<IElectionTypeService> electionTypeService = new();

            electionTypeService.Setup(service => service.DeleteElectionType(1))
                .ReturnsAsync(Result<string>.Success("Election type deleted successfully."));

            ElectionTypeController controller = new(electionTypeService.Object);

            IActionResult result = await controller.DeleteElectionType(1);

            ObjectResult actionResult = Assert.IsAssignableFrom<ObjectResult>(result);

            Assert.Equal(200, actionResult.StatusCode);

            electionTypeService.Verify(service => service.DeleteElectionType(1), Times.Once);
        }

        [Fact]
        public async Task GetPagedElectionTypes_ShouldReturnOk()
        {
            Mock<IElectionTypeService> electionTypeService = new();

            ElectionTypeRequest parameters = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<ElectionTypeResponse> response = new();

            electionTypeService.Setup(service => service.GetPagedElectionTypes(parameters))
                .ReturnsAsync(Result<PagedResponse<ElectionTypeResponse>>.Success(response));

            ElectionTypeController controller = new(electionTypeService.Object);

            IActionResult result = await controller.GetPagedElectionTypes(parameters);

            ObjectResult actionResult = Assert.IsAssignableFrom<ObjectResult>(result);

            Assert.Equal(200, actionResult.StatusCode);

            electionTypeService.Verify(service => service.GetPagedElectionTypes(parameters), Times.Once);
        }
    }
}