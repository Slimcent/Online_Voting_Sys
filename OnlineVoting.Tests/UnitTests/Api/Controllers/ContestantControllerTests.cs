using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineVoting.Api.Controllers;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Pagination;
using OnlineVoting.Models.Results;
using OnlineVoting.Services.Infrastructures;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Tests.Controllers
{
    public class ContestantControllerTests
    {
        [Fact]
        public async Task GetContestants_ReturnsOk_WhenContestantsAreRetrievedSuccessfully()
        {
            Mock<IContestantService> contestantService = new Mock<IContestantService>();
            ContestantController controller = new ContestantController(contestantService.Object);

            ContestantRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10
            };

            PagedResponse<ContestantResponse> response = new()
            {
                Items =
                [
                    new ContestantResponse
            {
                ContestantId = Guid.NewGuid().ToString(),
                PositionApplicationId = Guid.NewGuid().ToString(),
                ContestantName = "Test Student",
                RegistrationNumber = "REG001",
                ElectionName = "2026 Engineering Election",
                PositionName = "President",
                Active = true,
                CreatedAt = DateTime.UtcNow
            }
                ]
            };

            contestantService.Setup(x => x.GetContestants(request))
                .ReturnsAsync(Result<PagedResponse<ContestantResponse>>.Success(response));

            IActionResult actionResult = await controller.GetContestants(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);

            PagedResponse<ContestantResponse> result = Assert.IsType<PagedResponse<ContestantResponse>>(successResponse.Data);

            Assert.NotNull(result.Items);
            Assert.Single(result.Items);

            ContestantResponse contestant = result.Items.Single();

            Assert.Equal("Test Student", contestant.ContestantName);
            Assert.Equal("2026 Engineering Election", contestant.ElectionName);
            Assert.Equal("President", contestant.PositionName);
            Assert.True(contestant.Active);

            contestantService.Verify(x => x.GetContestants(request), Times.Once);
        }

        [Fact]
        public async Task ToggleContestantActivation_ReturnsOk_WhenContestantIsUpdatedSuccessfully()
        {
            Mock<IContestantService> contestantService = new Mock<IContestantService>();
            ContestantController controller = new ContestantController(contestantService.Object);

            string contestantId = Guid.NewGuid().ToString();

            contestantService.Setup(x => x.ToggleContestantActivation(contestantId))
                .ReturnsAsync(Result<string>.Success("Contestant activated successfully."));

            IActionResult actionResult = await controller.ToggleContestantActivation(contestantId);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            string result = Assert.IsType<string>(successResponse.Data);

            Assert.Equal("Contestant activated successfully.", result);

            contestantService.Verify(x => x.ToggleContestantActivation(contestantId), Times.Once);
        }
    }
}