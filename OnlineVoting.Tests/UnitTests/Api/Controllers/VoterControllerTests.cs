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

namespace OnlineVoting.Tests.Controllers
{
    public class VoterControllerTests
    {
        [Fact]
        public async Task RegisterVoter_WithValidRequest_ShouldReturnCreated()
        {
            Mock<IVoterService> voterService = new Mock<IVoterService>();

            RegisterVoterRequest request = new()
            {
                RegNumber = "REG001",
                ElectionId = Guid.NewGuid().ToString()
            };

            RegisteredVoterResponse response = new()
            {
                RegisteredVoterId = Guid.NewGuid().ToString(),
                StudentId = Guid.NewGuid(),
                RegistrationNumber = request.RegNumber,
                StudentName = "John Doe",
                ElectionId = request.ElectionId,
                ElectionName = "Student Election 2026",
                VotingCode = "ABC123",
                Active = true
            };

            voterService.Setup(service => service.RegisterVoter(request))
                .ReturnsAsync(Result<RegisteredVoterResponse>.Created(response));

            VoterController controller = new VoterController(voterService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            IActionResult result = await controller.RegisterVoter(request);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);

            RegisteredVoterResponse value = Assert.IsType<RegisteredVoterResponse>(objectResult.Value);

            Assert.Equal(response.RegisteredVoterId, value.RegisteredVoterId);
            Assert.Equal(response.StudentId, value.StudentId);
            Assert.Equal(response.RegistrationNumber, value.RegistrationNumber);
            Assert.Equal(response.StudentName, value.StudentName);
            Assert.Equal(response.ElectionId, value.ElectionId);
            Assert.Equal(response.ElectionName, value.ElectionName);
            Assert.Equal(response.VotingCode, value.VotingCode);
            Assert.Equal(response.Active, value.Active);

            voterService.Verify(service => service.RegisterVoter(request), Times.Once);
        }

        [Fact]
        public async Task GetRegisteredVoter_ReturnsOk_WhenRegisteredVoterExists()
        {
            Mock<IVoterService> voterService = new Mock<IVoterService>();

            string registeredVoterId = Guid.NewGuid().ToString();

            RegisteredVoterResponse response = new()
            {
                RegisteredVoterId = registeredVoterId,
                StudentId = Guid.NewGuid(),
                RegistrationNumber = "REG001",
                StudentName = "John Doe",
                ElectionId = Guid.NewGuid().ToString(),
                ElectionName = "Student Election 2026",
                VotingCode = "VOTE123",
                Active = true
            };

            voterService.Setup(x => x.GetRegisteredVoter(registeredVoterId))
                .ReturnsAsync(Result<RegisteredVoterResponse>.Success(response));

            VoterController controller = new VoterController(voterService.Object);

            IActionResult actionResult = await controller.GetRegisteredVoter(registeredVoterId);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            RegisteredVoterResponse result = Assert.IsType<RegisteredVoterResponse>(successResponse.Data);

            Assert.Equal(registeredVoterId, result.RegisteredVoterId);
            Assert.Equal(response.StudentId, result.StudentId);
            Assert.Equal(response.RegistrationNumber, result.RegistrationNumber);
            Assert.Equal(response.ElectionId, result.ElectionId);
            Assert.Equal(response.ElectionName, result.ElectionName);

            voterService.Verify(x => x.GetRegisteredVoter(registeredVoterId), Times.Once);
        }

        [Fact]
        public async Task GetRegisteredVoters_ReturnsOk_WhenRegisteredVotersExist()
        {
            Mock<IVoterService> voterService = new Mock<IVoterService>();

            RegisteredVoterRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = Guid.NewGuid().ToString()
            };

            RegisteredVoterResponse registeredVoter = new()
            {
                RegisteredVoterId = Guid.NewGuid().ToString(),
                StudentId = Guid.NewGuid(),
                RegistrationNumber = "REG001",
                StudentName = "John Doe",
                ElectionId = request.ElectionId,
                ElectionName = "Student Election 2026",
                VotingCode = "VOTE123",
                Active = true
            };

            PagedResponse<RegisteredVoterResponse> response = new()
            {
                Items = new List<RegisteredVoterResponse>
        {
            registeredVoter
        }
            };

            voterService.Setup(x => x.GetRegisteredVoters(request))
                .ReturnsAsync(Result<PagedResponse<RegisteredVoterResponse>>.Success(response));

            VoterController controller = new VoterController(voterService.Object);

            IActionResult actionResult = await controller.GetRegisteredVoters(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            PagedResponse<RegisteredVoterResponse> result =
                Assert.IsType<PagedResponse<RegisteredVoterResponse>>(successResponse.Data);

            Assert.NotNull(result.Items);
            Assert.Single(result.Items);

            RegisteredVoterResponse registeredVoterResponse = result.Items.Single();

            Assert.Equal(registeredVoter.RegisteredVoterId, registeredVoterResponse.RegisteredVoterId);
            Assert.Equal(registeredVoter.StudentId, registeredVoterResponse.StudentId);
            Assert.Equal(registeredVoter.ElectionId, registeredVoterResponse.ElectionId);

            voterService.Verify(x => x.GetRegisteredVoters(request), Times.Once);
        }

        [Fact]
        public async Task CastVote_ReturnsCreated_WhenVoteIsSuccessful()
        {
            Mock<IVoterService> voterService = new Mock<IVoterService>();

            CastVoteRequest request = new()
            {
                RegisteredVoterId = Guid.NewGuid().ToString(),
                VotingCode = "VOTE123",
                ElectionPositionId = Guid.NewGuid().ToString(),
                ContestantId = Guid.NewGuid().ToString()
            };

            voterService.Setup(x => x.CastVote(request))
                .ReturnsAsync(Result<string>.Created("Vote cast successfully."));

            VoterController controller = new VoterController(voterService.Object);

            IActionResult actionResult = await controller.CastVote(request);

            ObjectResult createdResult = Assert.IsType<ObjectResult>(actionResult);

            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            Assert.Equal("Vote cast successfully.", createdResult.Value);

            voterService.Verify(x => x.CastVote(request), Times.Once);
        }

        [Fact]
        public async Task GetMyVotes_ReturnsOk_WhenVotesAreRetrievedSuccessfully()
        {
            Mock<IVoterService> voterService = new Mock<IVoterService>();

            VoteHistoryRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = Guid.NewGuid().ToString(),
                ElectionPositionId = Guid.NewGuid().ToString()
            };

            PagedResponse<VoteHistoryResponse> response = new()
            {
                Items =
                [
                    new VoteHistoryResponse
            {
                VoteId = Guid.NewGuid().ToString(),
                ElectionId = request.ElectionId,
                ElectionName = "Student Election 2026",
                ElectionPositionId = request.ElectionPositionId,
                PositionName = "President",
                ContestantId = Guid.NewGuid().ToString(),
                ContestantName = "Jane Doe",
                VotedAt = DateTime.UtcNow
            }
                ]
            };

            voterService.Setup(x => x.GetMyVotes(request))
                .ReturnsAsync(Result<PagedResponse<VoteHistoryResponse>>.Success(response));

            VoterController controller = new VoterController(voterService.Object);

            IActionResult actionResult = await controller.GetMyVotes(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            PagedResponse<VoteHistoryResponse> result = Assert.IsType<PagedResponse<VoteHistoryResponse>>(successResponse.Data);

            Assert.Single(result.Items);
            Assert.Equal(response.Items.Single().VoteId, result.Items.Single().VoteId);

            voterService.Verify(x => x.GetMyVotes(request), Times.Once);
        }

        [Fact]
        public async Task GetElectionResults_ReturnsOk_WhenResultsAreRetrievedSuccessfully()
        {
            Mock<IVoterService> voterService = new Mock<IVoterService>();

            ElectionResultRequest request = new()
            {
                PageNumber = 1,
                PageSize = 10,
                ElectionId = Guid.NewGuid().ToString(),
                ElectionPositionId = Guid.NewGuid().ToString()
            };

            PagedResponse<ElectionResultResponse> response = new()
            {
                Items =
                [
                    new ElectionResultResponse
            {
                ElectionId = request.ElectionId,
                ElectionName = "Student Election 2026",
                ElectionPositionId = request.ElectionPositionId,
                PositionName = "President",
                ContestantId = Guid.NewGuid().ToString(),
                ContestantName = "Jane Doe",
                VoteCount = 3,
                TotalVotes = 5,
                Percentage = 60m
            }
                ]
            };

            voterService.Setup(x => x.GetElectionResults(request))
                .ReturnsAsync(Result<PagedResponse<ElectionResultResponse>>.Success(response));

            VoterController controller = new VoterController(voterService.Object);

            IActionResult actionResult = await controller.GetElectionResults(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult);
            SuccessResponse successResponse = Assert.IsType<SuccessResponse>(okResult.Value);
            PagedResponse<ElectionResultResponse> result = Assert.IsType<PagedResponse<ElectionResultResponse>>(successResponse.Data);

            Assert.Single(result.Items);
            Assert.Equal(response.Items.Single().ContestantId, result.Items.Single().ContestantId);
            Assert.Equal(3, result.Items.Single().VoteCount);
            Assert.Equal(5, result.Items.Single().TotalVotes);
            Assert.Equal(60m, result.Items.Single().Percentage);

            voterService.Verify(x => x.GetElectionResults(request), Times.Once);
        }
    }
}