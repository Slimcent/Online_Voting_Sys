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
    public class DepartmentControllerTests
    {
        private static DepartmentController CreateController(Mock<IDepartmentService> departmentService)
        {
            DepartmentController controller = new(departmentService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return controller;
        }

        [Fact]
        public async Task CreateDepartment_ShouldCallServiceAndReturnCreated()
        {
            Mock<IDepartmentService> departmentService = new();
            DepartmentController controller = CreateController(departmentService);

            CreateDepartmentRequest request = new()
            {
                Name = "Computer Engineering",
                FacultyId = 1
            };

            departmentService.Setup(service => service.CreateDepartment(request)).ReturnsAsync(Result<string>.Created("Department created successfully"));

            IActionResult result = await controller.CreateDepartment(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal("Department created successfully", objectResult.Value);

            departmentService.Verify(service => service.CreateDepartment(request), Times.Once);
        }

        [Fact]
        public async Task GetDepartments_ShouldCallServiceAndReturnPagedDepartments()
        {
            Mock<IDepartmentService> departmentService = new();
            DepartmentController controller = CreateController(departmentService);

            DepartmentRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<DepartmentResponse> response = new();

            departmentService.Setup(service => service.GetDepartments(parameters)).ReturnsAsync(Result<PagedResponse<DepartmentResponse>>.Success(response));

            IActionResult result = await controller.GetDepartments(parameters);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            departmentService.Verify(service => service.GetDepartments(parameters), Times.Once);
        }

        [Fact]
        public async Task GetDepartment_ShouldCallServiceAndReturnDepartment()
        {
            Mock<IDepartmentService> departmentService = new();
            DepartmentController controller = CreateController(departmentService);

            DepartmentResponse response = new()
            {
                Id = 1,
                Name = "Computer Engineering"
            };

            departmentService.Setup(service => service.GetDepartment(1)).ReturnsAsync(Result<DepartmentResponse>.Success(response));

            IActionResult result = await controller.GetDepartment(1);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            departmentService.Verify(service => service.GetDepartment(1), Times.Once);
        }

        [Fact]
        public async Task GetDepartmentsByFacultyId_ShouldCallServiceAndReturnDepartments()
        {
            Mock<IDepartmentService> departmentService = new();
            DepartmentController controller = CreateController(departmentService);

            IEnumerable<DepartmentResponse> response = new List<DepartmentResponse>
            {
                new()
                {
                    Id = 1,
                    Name = "Computer Engineering"
                }
            };

            departmentService.Setup(service => service.GetDepartmentsByFacultyId(1)).ReturnsAsync(Result<IEnumerable<DepartmentResponse>>.Success(response));

            IActionResult result = await controller.GetDepartmentsByFacultyId(1);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            departmentService.Verify(service => service.GetDepartmentsByFacultyId(1), Times.Once);
        }

        [Fact]
        public async Task GetDepartmentsByFacultyId_WithParameters_ShouldCallServiceAndReturnPagedDepartments()
        {
            Mock<IDepartmentService> departmentService = new();
            DepartmentController controller = CreateController(departmentService);

            DepartmentRequestParameters parameters = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<DepartmentResponse> response = new();

            departmentService.Setup(service => service.GetDepartmentsByFacultyId(1, parameters)).ReturnsAsync(Result<PagedResponse<DepartmentResponse>>.Success(response));

            IActionResult result = await controller.GetDepartmentsByFacultyId(1, parameters);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            departmentService.Verify(service => service.GetDepartmentsByFacultyId(1, parameters), Times.Once);
        }

        [Fact]
        public async Task UpdateDepartment_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IDepartmentService> departmentService = new();
            DepartmentController controller = CreateController(departmentService);

            CreateDepartmentRequest request = new()
            {
                Name = "Computer Science",
                FacultyId = 1
            };

            departmentService.Setup(service => service.UpdateDepartment(1, request)).ReturnsAsync(Result<string>.Success("Department updated successfully"));

            IActionResult result = await controller.UpdateDepartment(1, request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Department updated successfully", successResponse.Data);

            departmentService.Verify(service => service.UpdateDepartment(1, request), Times.Once);
        }

        [Fact]
        public async Task ToggleDepartmentActivation_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IDepartmentService> departmentService = new();
            DepartmentController controller = CreateController(departmentService);

            departmentService.Setup(service => service.ToggleDepartmentActivation(1)).ReturnsAsync(Result<string>.Success("Department deactivated successfully"));

            IActionResult result = await controller.ToggleDepartmentActivation(1);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Department deactivated successfully", successResponse.Data);

            departmentService.Verify(service => service.ToggleDepartmentActivation(1), Times.Once);
        }

        [Fact]
        public async Task DeleteDepartment_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IDepartmentService> departmentService = new();
            DepartmentController controller = CreateController(departmentService);

            departmentService.Setup(service => service.DeleteDepartment(1)).ReturnsAsync(Result<string>.Success("Department deleted successfully"));

            IActionResult result = await controller.DeleteDepartment(1);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Department deleted successfully", successResponse.Data);

            departmentService.Verify(service => service.DeleteDepartment(1), Times.Once);
        }
    }
}