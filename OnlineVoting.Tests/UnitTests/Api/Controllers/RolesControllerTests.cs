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
    public class RolesControllerTests
    {
        private static RolesController CreateController(Mock<IRolesService> roleService)
        {
            RolesController controller = new(roleService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return controller;
        }

        [Fact]
        public async Task GetAllRoles_ShouldCallServiceAndReturnRoles()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            IEnumerable<RoleResponse> response = new List<RoleResponse>
            {
                new()
                {
                    Id = "1",
                    Name = "Admin",
                    IsActive = true
                }
            };

            roleService.Setup(service => service.GetAllRoles()).ReturnsAsync(Result<IEnumerable<RoleResponse>>.Success(response));

            IActionResult result = await controller.GetAllRoles();

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            roleService.Verify(service => service.GetAllRoles(), Times.Once);
        }

        [Fact]
        public async Task GetAllActiveRoles_ShouldCallServiceAndReturnRoles()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            IEnumerable<RoleResponse> response = new List<RoleResponse>
            {
                new()
                {
                    Id = "1",
                    Name = "Admin",
                    IsActive = true
                }
            };

            roleService.Setup(service => service.GetAllActiveRoles()).ReturnsAsync(Result<IEnumerable<RoleResponse>>.Success(response));

            IActionResult result = await controller.GetAllActiveRoles();

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            roleService.Verify(service => service.GetAllActiveRoles(), Times.Once);
        }

        [Fact]
        public async Task GetAllDeactivatedRoles_ShouldCallServiceAndReturnRoles()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            IEnumerable<RoleResponse> response = new List<RoleResponse>
            {
                new()
                {
                    Id = "1",
                    Name = "Student",
                    IsActive = false
                }
            };

            roleService.Setup(service => service.GetAllDeactivatedRoles()).ReturnsAsync(Result<IEnumerable<RoleResponse>>.Success(response));

            IActionResult result = await controller.GetAllDeactivatedRoles();

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            roleService.Verify(service => service.GetAllDeactivatedRoles(), Times.Once);
        }

        [Fact]
        public async Task AllPagedRoles_ShouldCallServiceAndReturnPagedRoles()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            RoleRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<RoleResponse> response = new();

            roleService.Setup(service => service.AllRoles(request)).ReturnsAsync(Result<PagedResponse<RoleResponse>>.Success(response));

            IActionResult result = await controller.AllPagedRoles(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            roleService.Verify(service => service.AllRoles(request), Times.Once);
        }

        [Fact]
        public async Task AllPagedActiveRoles_ShouldCallServiceAndReturnPagedRoles()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            RoleRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<RoleResponse> response = new();

            roleService.Setup(service => service.AllActiveRoles(request)).ReturnsAsync(Result<PagedResponse<RoleResponse>>.Success(response));

            IActionResult result = await controller.AllPagedActiveRoles(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            roleService.Verify(service => service.AllActiveRoles(request), Times.Once);
        }

        [Fact]
        public async Task AllPagedDeactivatedRoles_ShouldCallServiceAndReturnPagedRoles()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            RoleRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<RoleResponse> response = new();

            roleService.Setup(service => service.AllDeactivatedRoles(request)).ReturnsAsync(Result<PagedResponse<RoleResponse>>.Success(response));

            IActionResult result = await controller.AllPagedDeactivatedRoles(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            roleService.Verify(service => service.AllDeactivatedRoles(request), Times.Once);
        }

        [Fact]
        public async Task GetUserRoles_ShouldCallServiceAndReturnRoles()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            IList<string> response = new List<string>
            {
                "Admin",
                "ElectionManager"
            };

            roleService.Setup(service => service.GetUserRoles("user@example.com")).ReturnsAsync(Result<IList<string>>.Success(response));

            IActionResult result = await controller.GetUserRoles("user@example.com");

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            roleService.Verify(service => service.GetUserRoles("user@example.com"), Times.Once);
        }

        [Fact]
        public async Task CreateRole_ShouldCallServiceAndReturnCreated()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            CreateRoleRequest request = new()
            {
                Name = "Admin"
            };

            roleService.Setup(service => service.CreateRole(request)).ReturnsAsync(Result<string>.Created("Role created successfully"));

            IActionResult result = await controller.CreateRole(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal("Role created successfully", objectResult.Value);

            roleService.Verify(service => service.CreateRole(request), Times.Once);
        }

        [Fact]
        public async Task EditRole_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            CreateRoleRequest request = new()
            {
                Name = "Administrator"
            };

            roleService.Setup(service => service.EditRole("role-id", request)).ReturnsAsync(Result<string>.Success("Role updated successfully"));

            IActionResult result = await controller.EditRole("role-id", request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Role updated successfully", successResponse.Data);

            roleService.Verify(service => service.EditRole("role-id", request), Times.Once);
        }

        [Fact]
        public async Task AddUserToRole_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            AddUserToRoleRequest request = new()
            {
                Email = "user@example.com",
                Name = "Admin"
            };

            roleService.Setup(service => service.AddUserToRole(request)).ReturnsAsync(Result<string>.Success("User added to role successfully"));

            IActionResult result = await controller.AddUserToRole(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("User added to role successfully", successResponse.Data);

            roleService.Verify(service => service.AddUserToRole(request), Times.Once);
        }

        [Fact]
        public async Task RemoveUserFromRole_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            AddUserToRoleRequest request = new()
            {
                Email = "user@example.com",
                Name = "Admin"
            };

            roleService.Setup(service => service.RemoveUserFromRole(request)).ReturnsAsync(Result<string>.Success("User removed from role successfully"));

            IActionResult result = await controller.RemoveUserFromRole(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("User removed from role successfully", successResponse.Data);

            roleService.Verify(service => service.RemoveUserFromRole(request), Times.Once);
        }

        [Fact]
        public async Task ToggleRoleStatus_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            roleService.Setup(service => service.ToggleRoleStatus("role-id")).ReturnsAsync(Result<string>.Success("Role deactivated successfully"));

            IActionResult result = await controller.ToggleRoleStatus("role-id");

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Role deactivated successfully", successResponse.Data);

            roleService.Verify(service => service.ToggleRoleStatus("role-id"), Times.Once);
        }

        [Fact]
        public async Task DeleteUserRole_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            roleService.Setup(service => service.DeleteUserRole("role-id")).ReturnsAsync(Result<string>.Success("Role deleted successfully"));

            IActionResult result = await controller.DeleteUserRole("role-id");

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Role deleted successfully", successResponse.Data);

            roleService.Verify(service => service.DeleteUserRole("role-id"), Times.Once);
        }

        [Fact]
        public async Task DeleteRole_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IRolesService> roleService = new();
            RolesController controller = CreateController(roleService);

            CreateRoleRequest request = new()
            {
                Name = "Admin"
            };

            roleService.Setup(service => service.DeleteRole(request)).ReturnsAsync(Result<string>.Success("Role deleted successfully"));

            IActionResult result = await controller.DeleteRole(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("Role deleted successfully", successResponse.Data);

            roleService.Verify(service => service.DeleteRole(request), Times.Once);
        }
    }
}