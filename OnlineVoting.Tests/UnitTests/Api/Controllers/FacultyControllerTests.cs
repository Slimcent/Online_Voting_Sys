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

namespace OnlineVoting.Tests.UnitTests.Api.Controllers
{
    public class FacultyControllerTests
    {
        private static FacultyController CreateController(Mock<IFacultyService> facultyService)
        {
            FacultyController controller = new(facultyService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return controller;
        }

        [Fact]
        public async Task CreateFaculty_ShouldCallServiceAndReturnCreated()
        {
            Mock<IFacultyService> facultyService = new();
            FacultyController controller = CreateController(facultyService);

            CreateFacultyRequest request = new()
            {
                Name = "Engineering"
            };

            facultyService.Setup(service => service.CreateFaculty(request)).ReturnsAsync(Result<string>.Created("Faculty created successfully"));

            IActionResult result = await controller.CreateFaculty(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal("Faculty created successfully", objectResult.Value);

            facultyService.Verify(service => service.CreateFaculty(request), Times.Once);
        }

        [Fact]
        public async Task GetFaculties_ShouldCallServiceAndReturnPagedFaculties()
        {
            Mock<IFacultyService> facultyService = new();
            FacultyController controller = CreateController(facultyService);

            FacultyRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<FacultyResponse> response = new();

            facultyService.Setup(service => service.GetFaculties(parameters)).ReturnsAsync(Result<PagedResponse<FacultyResponse>>.Success(response));

            IActionResult result = await controller.GetFaculties(parameters);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            facultyService.Verify(service => service.GetFaculties(parameters), Times.Once);
        }

        [Fact]
        public async Task GetFaculty_ShouldCallServiceAndReturnFaculty()
        {
            Mock<IFacultyService> facultyService = new();
            FacultyController controller = CreateController(facultyService);

            FacultyResponse response = new()
            {
                Id = 1,
                Name = "Engineering"
            };

            facultyService.Setup(service => service.GetFaculty(1)).ReturnsAsync(Result<FacultyResponse>.Success(response));

            IActionResult result = await controller.GetFaculty(1);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            facultyService.Verify(service => service.GetFaculty(1), Times.Once);
        }

        [Fact]
        public async Task UpdateFaculty_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IFacultyService> facultyService = new();
            FacultyController controller = CreateController(facultyService);

            CreateWithNameRequest request = new()
            {
                Name = "Science"
            };

            facultyService.Setup(service => service.UpdateFaculty(1, request)).ReturnsAsync(Result<string>.Success("Faculty updated successfully"));

            IActionResult result = await controller.UpdateFaculty(1, request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Faculty updated successfully", successResponse.Data);

            facultyService.Verify(service => service.UpdateFaculty(1, request), Times.Once);
        }

        [Fact]
        public async Task ToggleFacultyActivation_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IFacultyService> facultyService = new();
            FacultyController controller = CreateController(facultyService);

            facultyService.Setup(service => service.ToggleFacultyActivation(1)).ReturnsAsync(Result<string>.Success("Faculty deactivated successfully"));

            IActionResult result = await controller.ToggleFacultyActivation(1);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Faculty deactivated successfully", successResponse.Data);

            facultyService.Verify(service => service.ToggleFacultyActivation(1), Times.Once);
        }

        [Fact]
        public async Task DeleteFaculty_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IFacultyService> facultyService = new();
            FacultyController controller = CreateController(facultyService);

            facultyService.Setup(service => service.DeleteFaculty(1)).ReturnsAsync(Result<string>.Success("Faculty deleted successfully"));

            IActionResult result = await controller.DeleteFaculty(1);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Faculty deleted successfully", successResponse.Data);

            facultyService.Verify(service => service.DeleteFaculty(1), Times.Once);
        }

        [Fact]
        public async Task GetFacultiesWithDepartments_ShouldCallServiceAndReturnPagedFaculties()
        {
            Mock<IFacultyService> facultyService = new();
            FacultyController controller = CreateController(facultyService);

            FacultyRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<FacultyResponse> response = new();

            facultyService.Setup(service => service.GetFacultiesWithDepartments(parameters)).ReturnsAsync(Result<PagedResponse<FacultyResponse>>.Success(response));

            IActionResult result = await controller.GetFacultiesWithDepartments(parameters);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            facultyService.Verify(service => service.GetFacultiesWithDepartments(parameters), Times.Once);
        }

        [Fact]
        public async Task GetFacultyWithDepartments_ShouldCallServiceAndReturnFaculty()
        {
            Mock<IFacultyService> facultyService = new();
            FacultyController controller = CreateController(facultyService);

            FacultyResponse response = new()
            {
                Id = 1,
                Name = "Engineering"
            };

            facultyService.Setup(service => service.GetFacultyWithDepartments(1)).ReturnsAsync(Result<FacultyResponse>.Success(response));

            IActionResult result = await controller.GetFacultyWithDepartments(1);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            facultyService.Verify(service => service.GetFacultyWithDepartments(1), Times.Once);
        }
    }
}