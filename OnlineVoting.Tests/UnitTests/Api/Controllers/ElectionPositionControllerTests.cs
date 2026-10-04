using Microsoft.AspNetCore.Http;
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

namespace OnlineVoting.Tests.UnitTests.Controllers
{
    public class ElectionPositionControllerTests
    {
        [Fact]
        public async Task GetElectionPositions_WithSuccessfulResult_ShouldReturnOkResponse()
        {
            Mock<IElectionPositionService> electionPositionService = new();

            ElectionPositionRequest request = new()
            {
                ElectionId = Guid.NewGuid().ToString(),
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<ElectionPositionResponse> response = new()
            {
                Items = new List<ElectionPositionResponse>
                {
                    new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ElectionId = request.ElectionId,
                        Election = "2026 Department Election",
                        PositionId = Guid.NewGuid().ToString(),
                        Position = "President",
                        ApplicationFee = 1000,
                        Currency = "NGN",
                        Active = true,
                        NumberOfApplications = 2
                    }
                }
            };

            electionPositionService.Setup(service => service.GetElectionPositions(request))
                .ReturnsAsync(Result<PagedResponse<ElectionPositionResponse>>.Success(response));

            ElectionPositionController controller = new(electionPositionService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.GetElectionPositions(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.Same(response, successResponse.Data);

            electionPositionService.Verify(service => service.GetElectionPositions(request), Times.Once);
        }

        [Fact]
        public async Task GetElectionPositionsWithApplications_WithSuccessfulResult_ShouldReturnOkResponse()
        {
            Mock<IElectionPositionService> electionPositionService = new();

            ElectionPositionRequest request = new()
            {
                ElectionId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = 3,
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<ElectionPositionWithApplicationsResponse> response = new()
            {
                Items = new List<ElectionPositionWithApplicationsResponse>
                {
                    new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ElectionId = request.ElectionId,
                        Election = "2026 Department Election",
                        PositionId = Guid.NewGuid().ToString(),
                        Position = "President",
                        ApplicationFee = 1000,
                        Currency = "NGN",
                        Active = true,
                        NumberOfApplications = 1,
                        Applications = new List<ElectionPositionApplicationResponse>
                        {
                            new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                StudentId = Guid.NewGuid(),
                                FirstName = "John",
                                LastName = "Doe",
                                RegNumber = "CE/2026/001",
                                DepartmentId = 1,
                                Department = "Computer Engineering",
                                FacultyId = 1,
                                Faculty = "Engineering",
                                PositionApplicationStatusId = 3,
                                PositionApplicationStatus = "Approved",
                                Active = true
                            }
                        }
                    }
                }
            };

            electionPositionService.Setup(service => service.GetElectionPositionsWithApplications(request))
                .ReturnsAsync(Result<PagedResponse<ElectionPositionWithApplicationsResponse>>.Success(response));

            ElectionPositionController controller = new(electionPositionService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.GetElectionPositionsWithApplications(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.Same(response, successResponse.Data);

            electionPositionService.Verify(service => service.GetElectionPositionsWithApplications(request), Times.Once);
        }

        [Fact]
        public async Task CreateElectionPosition_WithSuccessfulResult_ShouldReturnCreated()
        {
            Mock<IElectionPositionService> electionPositionService = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();
            string response = "Election position created successfully.";

            electionPositionService.Setup(service => service.CreateElectionPosition(request))
                .ReturnsAsync(Result<string>.Created(response));

            ElectionPositionController controller = new(electionPositionService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.CreateElectionPosition(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal(response, objectResult.Value);

            electionPositionService.Verify(service => service.CreateElectionPosition(request), Times.Once);
        }

        [Fact]
        public async Task CreateElectionPositions_WithSuccessfulResult_ShouldReturnCreated()
        {
            Mock<IElectionPositionService> electionPositionService = new();

            CreateElectionPositionsRequest request = ElectionPositionTestData.CreateElectionPositionsRequest();
            string response = "Election positions created successfully.";

            electionPositionService.Setup(service => service.CreateElectionPositions(request))
                .ReturnsAsync(Result<string>.Created(response));

            ElectionPositionController controller = new(electionPositionService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.CreateElectionPositions(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal(response, objectResult.Value);

            electionPositionService.Verify(service => service.CreateElectionPositions(request), Times.Once);
        }

        [Fact]
        public async Task UpdateElectionPosition_WithSuccessfulResult_ShouldReturnOkResponse()
        {
            Mock<IElectionPositionService> electionPositionService = new();

            CreateElectionPositionRequest request = ElectionPositionTestData.CreateElectionPositionRequest();
            request.Id = Guid.NewGuid().ToString();

            string response = "Election position updated successfully.";

            electionPositionService.Setup(service => service.UpdateElectionPosition(request))
                .ReturnsAsync(Result<string>.Success(response));

            ElectionPositionController controller = new(electionPositionService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.UpdateElectionPosition(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.Equal(response, successResponse.Data);

            electionPositionService.Verify(service => service.UpdateElectionPosition(request), Times.Once);
        }

        [Fact]
        public async Task DeleteElectionPosition_WithSuccessfulResult_ShouldReturnOkResponse()
        {
            Mock<IElectionPositionService> electionPositionService = new();

            string id = Guid.NewGuid().ToString();
            string response = "Election position deleted successfully.";

            electionPositionService.Setup(service => service.DeleteElectionPosition(id))
                .ReturnsAsync(Result<string>.Success(response));

            ElectionPositionController controller = new(electionPositionService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.DeleteElectionPosition(id);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.Equal(response, successResponse.Data);

            electionPositionService.Verify(service => service.DeleteElectionPosition(id), Times.Once);
        }

        [Fact]
        public async Task ToggleElectionPositionActivation_WithSuccessfulResult_ShouldReturnOkResponse()
        {
            Mock<IElectionPositionService> electionPositionService = new();

            string id = Guid.NewGuid().ToString();
            string response = "Election position deactivated successfully.";

            electionPositionService.Setup(service => service.ToggleElectionPositionActivation(id))
                .ReturnsAsync(Result<string>.Success(response));

            ElectionPositionController controller = new(electionPositionService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.ToggleElectionPositionActivation(id);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.Equal(response, successResponse.Data);

            electionPositionService.Verify(service => service.ToggleElectionPositionActivation(id), Times.Once);
        }
    }
}