using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineVoting.Api.Controllers;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Tests.UnitTests.Api.Controllers
{
    public class ClaimsControllerTests
    {
        private static ClaimsController CreateController(Mock<IClaimsService> claimsService)
        {
            ClaimsController controller = new(claimsService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return controller;
        }

        [Fact]
        public async Task AddUserToClaims_ShouldCallServiceAndReturnCreated()
        {
            Mock<IClaimsService> claimsService = new();
            ClaimsController controller = CreateController(claimsService);

            UserClaimsRequest request = new()
            {
                Email = "user@example.com",
                ClaimType = "Permission",
                ClaimValue = "ManageElection"
            };

            UserClaimsResponse response = new()
            {
                Email = request.Email,
                ClaimType = request.ClaimType,
                ClaimValue = request.ClaimValue
            };

            claimsService.Setup(service => service.CreateUserClaims(request)).ReturnsAsync(Result<UserClaimsResponse>.Created(response));

            IActionResult result = await controller.AddUserToClaims(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Same(response, objectResult.Value);

            claimsService.Verify(service => service.CreateUserClaims(request), Times.Once);
        }

        [Fact]
        public async Task DeleteClaim_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IClaimsService> claimsService = new();
            ClaimsController controller = CreateController(claimsService);

            UserClaimsRequest request = new()
            {
                Email = "user@example.com",
                ClaimType = "Permission",
                ClaimValue = "ManageElection"
            };

            claimsService.Setup(service => service.DeleteClaims(request)).ReturnsAsync(Result<string>.Success("User removed from claim successfully"));

            IActionResult result = await controller.DeleteClaim(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Equal("User removed from claim successfully", successResponse.Data);

            claimsService.Verify(service => service.DeleteClaims(request), Times.Once);
        }

        [Fact]
        public async Task EditClaim_ShouldCallServiceAndReturnSuccess()
        {
            Mock<IClaimsService> claimsService = new();
            ClaimsController controller = CreateController(claimsService);

            UserClaimsRequest request = new()
            {
                Email = "user@example.com",
                ClaimType = "Permission",
                ClaimValue = "ManageElection",
                OldValue = "CreateElection"
            };

            UserClaimsResponse response = new()
            {
                Email = request.Email,
                ClaimType = request.ClaimType,
                ClaimValue = request.ClaimValue,
                OldValue = request.OldValue
            };

            claimsService.Setup(service => service.EditUserClaims(request)).ReturnsAsync(Result<UserClaimsResponse>.Success(response));

            IActionResult result = await controller.EditClaim(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            claimsService.Verify(service => service.EditUserClaims(request), Times.Once);
        }

        [Fact]
        public async Task GetUserClaims_ShouldCallServiceAndReturnClaims()
        {
            Mock<IClaimsService> claimsService = new();
            ClaimsController controller = CreateController(claimsService);

            string email = "user@example.com";

            IEnumerable<UserClaimsResponse> response = new List<UserClaimsResponse>
            {
                new()
                {
                    ClaimType = "Permission",
                    ClaimValue = "ManageElection"
                }
            };

            claimsService.Setup(service => service.GetUserClaims(email)).ReturnsAsync(Result<IEnumerable<UserClaimsResponse>>.Success(response));

            IActionResult result = await controller.GetUserClaims(email);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            Assert.True(successResponse.Success);
            Assert.Same(response, successResponse.Data);

            claimsService.Verify(service => service.GetUserClaims(email), Times.Once);
        }
    }
}