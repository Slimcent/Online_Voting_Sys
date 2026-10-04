using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineVoting.Api.Controllers;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Tests.Controllers
{
    public class PositionApplicationControllerTests
    {
        [Fact]
        public async Task CreatePositionApplication_WithValidRequest_ShouldReturnCreated()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();

            CreatePositionApplicationRequest request = new CreatePositionApplicationRequest
            {
                ElectionPositionId = Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            CreatePositionApplicationResponse response = new CreatePositionApplicationResponse
            {
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceId = Guid.NewGuid().ToString(),
                InvoiceNumber = "INV-001",
                Amount = 5000,
                Currency = "NGN",
                ApplicationStatus = "Pending Payment",
                InvoiceStatus = "Unpaid"
            };

            positionApplicationService.Setup(service => service.CreatePositionApplication(request))
                .ReturnsAsync(Result<CreatePositionApplicationResponse>.Created(response));

            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.CreatePositionApplication(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);

            CreatePositionApplicationResponse value = Assert.IsType<CreatePositionApplicationResponse>(objectResult.Value);

            Assert.Equal(response.PositionApplicationId, value.PositionApplicationId);
            Assert.Equal(response.InvoiceId, value.InvoiceId);
            Assert.Equal(response.InvoiceNumber, value.InvoiceNumber);
            Assert.Equal(response.Amount, value.Amount);
            Assert.Equal(response.Currency, value.Currency);
            Assert.Equal(response.ApplicationStatus, value.ApplicationStatus);
            Assert.Equal(response.InvoiceStatus, value.InvoiceStatus);

            positionApplicationService.Verify(service => service.CreatePositionApplication(request), Times.Once);
        }

        [Fact]
        public async Task CreatePositionApplication_WithExistingApplication_ShouldReturnOk()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();

            CreatePositionApplicationRequest request = new CreatePositionApplicationRequest
            {
                ElectionPositionId = Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            CreatePositionApplicationResponse response = new CreatePositionApplicationResponse
            {
                PositionApplicationId = Guid.NewGuid().ToString(),
                InvoiceId = Guid.NewGuid().ToString(),
                InvoiceNumber = "INV-001",
                Amount = 5000,
                Currency = "NGN",
                ApplicationStatus = "Pending Payment",
                InvoiceStatus = "Unpaid"
            };

            positionApplicationService.Setup(service => service.CreatePositionApplication(request))
                .ReturnsAsync(Result<CreatePositionApplicationResponse>.Success(response));

            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.CreatePositionApplication(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);

            CreatePositionApplicationResponse value = Assert.IsType<CreatePositionApplicationResponse>(okResult.Value);

            Assert.Equal(response.PositionApplicationId, value.PositionApplicationId);
            Assert.Equal(response.InvoiceId, value.InvoiceId);
            Assert.Equal(response.InvoiceNumber, value.InvoiceNumber);
            Assert.Equal(response.Amount, value.Amount);
            Assert.Equal(response.Currency, value.Currency);
            Assert.Equal(response.ApplicationStatus, value.ApplicationStatus);
            Assert.Equal(response.InvoiceStatus, value.InvoiceStatus);

            positionApplicationService.Verify(service => service.CreatePositionApplication(request), Times.Once);
        }

        [Fact]
        public async Task CreatePositionApplication_WithValidationError_ShouldReturnBadRequest()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();

            CreatePositionApplicationRequest request = new CreatePositionApplicationRequest
            {
                ElectionPositionId = Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            positionApplicationService.Setup(service => service.CreatePositionApplication(request))
                .ReturnsAsync(Result<CreatePositionApplicationResponse>.ValidationError("The application period has ended."));

            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.CreatePositionApplication(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);

            positionApplicationService.Verify(service => service.CreatePositionApplication(request), Times.Once);
        }

        [Fact]
        public async Task CreatePositionApplication_WhenResourceIsNotFound_ShouldReturnNotFound()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();

            CreatePositionApplicationRequest request = new CreatePositionApplicationRequest
            {
                ElectionPositionId = Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            positionApplicationService.Setup(service => service.CreatePositionApplication(request))
                .ReturnsAsync(Result<CreatePositionApplicationResponse>.NotFound("Election position was not found."));

            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.CreatePositionApplication(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);

            positionApplicationService.Verify(service => service.CreatePositionApplication(request), Times.Once);
        }

        [Fact]
        public async Task CreatePositionApplication_WhenConflictOccurs_ShouldReturnConflict()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();

            CreatePositionApplicationRequest request = new CreatePositionApplicationRequest
            {
                ElectionPositionId = Guid.NewGuid().ToString(),
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            positionApplicationService.Setup(service => service.CreatePositionApplication(request))
                .ReturnsAsync(Result<CreatePositionApplicationResponse>.Conflict("The request is already being processed."));

            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.CreatePositionApplication(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);

            positionApplicationService.Verify(service => service.CreatePositionApplication(request), Times.Once);
        }

        [Fact]
        public async Task GetMyPositionApplications_ReturnsOk_WhenApplicationsAreRetrievedSuccessfully()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();
            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object);

            PositionApplicationRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<PositionApplicationResponse> response = new()
            {
                Items =
                [
                    new PositionApplicationResponse
            {
                PositionApplicationId = Guid.NewGuid().ToString(),
                ElectionPositionId = Guid.NewGuid().ToString(),
                ElectionName = "2026 Engineering Election",
                PositionName = "President",
                ApplicationStatus = "Pending Payment",
                CreatedAt = DateTime.UtcNow
            }
                ]
            };

            positionApplicationService.Setup(x => x.GetMyPositionApplications(request))
                .ReturnsAsync(Result<PagedResponse<PositionApplicationResponse>>.Success(response));

            IActionResult actionResult = await controller.GetMyPositionApplications(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            PagedResponse<PositionApplicationResponse> result = Assert.IsType<PagedResponse<PositionApplicationResponse>>(okResult.Value);

            Assert.Single(result.Items);
            Assert.Equal(response.Items.Single().PositionApplicationId, result.Items.Single().PositionApplicationId);

            positionApplicationService.Verify(x => x.GetMyPositionApplications(request), Times.Once);
        }

        [Fact]
        public async Task GetMyPositionApplication_ReturnsOk_WhenApplicationIsRetrievedSuccessfully()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();
            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object);

            string positionApplicationId = Guid.NewGuid().ToString();

            PositionApplicationResponse response = new()
            {
                PositionApplicationId = positionApplicationId,
                StudentId = Guid.NewGuid(),
                RegistrationNumber = "REG001",
                StudentName = "Test Student",
                StudentEmail = "student@example.com",
                ElectionPositionId = Guid.NewGuid().ToString(),
                ElectionName = "2026 Engineering Election",
                PositionName = "President",
                ApplicationStatus = "Pending Review",
                CreatedAt = DateTime.UtcNow
            };

            positionApplicationService.Setup(x => x.GetPositionApplication(positionApplicationId))
                .ReturnsAsync(Result<PositionApplicationResponse>.Success(response));

            IActionResult actionResult = await controller.GetMyPositionApplication(positionApplicationId);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            PositionApplicationResponse result = Assert.IsType<PositionApplicationResponse>(okResult.Value);

            Assert.Equal(positionApplicationId, result.PositionApplicationId);
            Assert.Equal(response.StudentId, result.StudentId);
            Assert.Equal(response.RegistrationNumber, result.RegistrationNumber);
            Assert.Equal(response.StudentName, result.StudentName);
            Assert.Equal(response.StudentEmail, result.StudentEmail);
            Assert.Equal(response.ElectionPositionId, result.ElectionPositionId);
            Assert.Equal(response.ElectionName, result.ElectionName);
            Assert.Equal(response.PositionName, result.PositionName);
            Assert.Equal(response.ApplicationStatus, result.ApplicationStatus);

            positionApplicationService.Verify(x => x.GetPositionApplication(positionApplicationId), Times.Once);
        }

        [Fact]
        public async Task GetPositionApplication_ReturnsOk_WhenApplicationIsRetrievedSuccessfully()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();
            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object);

            string positionApplicationId = Guid.NewGuid().ToString();

            PositionApplicationResponse response = new()
            {
                PositionApplicationId = positionApplicationId,
                StudentId = Guid.NewGuid(),
                RegistrationNumber = "REG001",
                StudentName = "Test Student",
                StudentEmail = "student@example.com",
                ElectionPositionId = Guid.NewGuid().ToString(),
                ElectionName = "2026 Engineering Election",
                PositionName = "President",
                ApplicationStatus = "Pending Review",
                CreatedAt = DateTime.UtcNow
            };

            positionApplicationService.Setup(x => x.GetPositionApplication(positionApplicationId))
                .ReturnsAsync(Result<PositionApplicationResponse>.Success(response));

            IActionResult actionResult = await controller.GetPositionApplication(positionApplicationId);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            PositionApplicationResponse result = Assert.IsType<PositionApplicationResponse>(okResult.Value);

            Assert.Equal(positionApplicationId, result.PositionApplicationId);
            Assert.Equal(response.StudentId, result.StudentId);
            Assert.Equal(response.RegistrationNumber, result.RegistrationNumber);
            Assert.Equal(response.StudentName, result.StudentName);
            Assert.Equal(response.StudentEmail, result.StudentEmail);
            Assert.Equal(response.ElectionPositionId, result.ElectionPositionId);
            Assert.Equal(response.ElectionName, result.ElectionName);
            Assert.Equal(response.PositionName, result.PositionName);
            Assert.Equal(response.ApplicationStatus, result.ApplicationStatus);

            positionApplicationService.Verify(x => x.GetPositionApplication(positionApplicationId), Times.Once);
        }

        [Fact]
        public async Task GetPositionApplications_ReturnsOk_WhenApplicationsAreRetrievedSuccessfully()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();
            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object);

            PositionApplicationRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<PositionApplicationResponse> response = new()
            {
                Items =
                [
                    new PositionApplicationResponse
            {
                PositionApplicationId = Guid.NewGuid().ToString(),
                StudentId = Guid.NewGuid(),
                RegistrationNumber = "REG001",
                StudentName = "Test Student",
                StudentEmail = "student@example.com",
                ElectionPositionId = Guid.NewGuid().ToString(),
                ElectionName = "2026 Engineering Election",
                PositionName = "President",
                ApplicationStatus = "Pending Review",
                CreatedAt = DateTime.UtcNow
            }
                ]
            };

            positionApplicationService.Setup(x => x.GetPositionApplications(request))
                .ReturnsAsync(Result<PagedResponse<PositionApplicationResponse>>.Success(response));

            IActionResult actionResult = await controller.GetPositionApplications(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            PagedResponse<PositionApplicationResponse> result = Assert.IsType<PagedResponse<PositionApplicationResponse>>(okResult.Value);

            Assert.NotNull(result.Items);
            Assert.Single(result.Items);

            positionApplicationService.Verify(x => x.GetPositionApplications(request), Times.Once);
        }

        [Fact]
        public async Task ApproveOrRejectPositionApplication_ReturnsOk_WhenApplicationIsProcessedSuccessfully()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();
            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object);

            ApproveOrRejectPositionApplicationRequest request = new()
            {
                PositionApplicationId = Guid.NewGuid().ToString(),
                PositionApplicationStatusId = 3
            };

            positionApplicationService.Setup(x => x.ApproveOrRejectPositionApplication(request))
                .ReturnsAsync(Result<string>.Success("Position application approved successfully."));

            IActionResult actionResult = await controller.ApproveOrRejectPositionApplication(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            string result = Assert.IsType<string>(okResult.Value);

            Assert.Equal("Position application approved successfully.", result);

            positionApplicationService.Verify(x => x.ApproveOrRejectPositionApplication(request), Times.Once);
        }

        [Fact]
        public async Task GetPositionApplicationsWithContestants_ReturnsOk_WhenApplicationsAreRetrievedSuccessfully()
        {
            Mock<IPositionApplicationService> positionApplicationService = new Mock<IPositionApplicationService>();
            PositionApplicationController controller = new PositionApplicationController(positionApplicationService.Object);

            PositionApplicationWithContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<PositionApplicationResponse> response = new()
            {
                Items =
                [
                    new PositionApplicationResponse
                    {
                        PositionApplicationId = Guid.NewGuid().ToString(),
                        StudentId = Guid.NewGuid(),
                        RegistrationNumber = "REG001",
                        StudentName = "Test Student",
                        StudentEmail = "student@example.com",
                        ElectionPositionId = Guid.NewGuid().ToString(),
                        ElectionName = "2026 Engineering Election",
                        PositionName = "President",
                        ApplicationStatus = "Approved",
                        Contestant = new ContestantResponse
                        {
                            ContestantId = Guid.NewGuid().ToString(),
                            PositionApplicationId = Guid.NewGuid().ToString(),
                            ContestantName = "Test Student",
                            RegistrationNumber = "REG001",
                            PositionName = "President",
                            Active = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        CreatedAt = DateTime.UtcNow
                    }
                ]
            };

            positionApplicationService.Setup(x => x.GetPositionApplicationsWithContestants(request))
                .ReturnsAsync(Result<PagedResponse<PositionApplicationResponse>>.Success(response));

            IActionResult actionResult = await controller.GetPositionApplicationsWithContestants(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            PagedResponse<PositionApplicationResponse> result = Assert.IsType<PagedResponse<PositionApplicationResponse>>(okResult.Value);

            Assert.NotNull(result.Items);
            Assert.Single(result.Items);

            PositionApplicationResponse positionApplication = result.Items.Single();

            Assert.NotNull(positionApplication.Contestant);
            Assert.Equal("Test Student", positionApplication.Contestant.ContestantName);
            Assert.Equal("President", positionApplication.Contestant.PositionName);

            positionApplicationService.Verify(x => x.GetPositionApplicationsWithContestants(request), Times.Once);
        }
    }
}