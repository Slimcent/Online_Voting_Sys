using Microsoft.AspNetCore.Identity;
using Moq;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Models.Dtos.Response;
using OnlineVoting.Models.Entities;
using OnlineVoting.Models.Results;
using OnlineVoting.Tests.TestData.Data;
using OnlineVoting.Tests.TestData.Factories;
using System.Security.Claims;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using System.Net;
using System.Text;

namespace OnlineVoting.Tests.UnitTests.Services
{
    public class ClaimsServiceTests
    {
        [Fact]
        public async Task CreateUserClaims_WithEmptyEmail_ShouldReturnValidationError()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(email: " ");

            Result<UserClaimsResponse> result = await factory.Service.CreateUserClaims(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Email cannot be empty", result.Error);

            factory.UserManager.Verify(userManager => userManager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserClaims_WithEmptyClaimType_ShouldReturnValidationError()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(claimType: " ");

            Result<UserClaimsResponse> result = await factory.Service.CreateUserClaims(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Claim type cannot be empty", result.Error);

            factory.UserManager.Verify(userManager => userManager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserClaims_WithEmptyClaimValue_ShouldReturnValidationError()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(claimValue: " ");

            Result<UserClaimsResponse> result = await factory.Service.CreateUserClaims(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Claim value cannot be empty", result.Error);

            factory.UserManager.Verify(userManager => userManager.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserClaims_WithMissingUser_ShouldReturnNotFound()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(email: " user@example.com ");

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com"))
                .ReturnsAsync((User?)null);

            Result<UserClaimsResponse> result = await factory.Service.CreateUserClaims(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("User with email  user@example.com  was not found", result.Error);

            factory.UserManager.Verify(userManager => userManager.GetClaimsAsync(It.IsAny<User>()), Times.Never);
            factory.UserManager.Verify(userManager => userManager.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserClaims_WithExistingClaim_ShouldReturnConflict()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest();
            User user = ClaimsTestData.CreateUser();

            IList<Claim> claims = new List<Claim>
            {
                ClaimsTestData.CreateClaim()
            };

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(user);

            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user))
                .ReturnsAsync(claims);

            Result<UserClaimsResponse> result = await factory.Service.CreateUserClaims(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("The user already has this claim", result.Error);

            factory.UserManager.Verify(userManager => userManager.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserClaims_WhenIdentityCreationFails_ShouldReturnValidationError()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest();
            User user = ClaimsTestData.CreateUser();

            IdentityError error = new()
            {
                Description = "Unable to add claim."
            };

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(user);

            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user))
                .ReturnsAsync(new List<Claim>());

            factory.UserManager.Setup(userManager => userManager.AddClaimAsync(user, It.IsAny<Claim>()))
                .ReturnsAsync(IdentityResult.Failed(error));

            Result<UserClaimsResponse> result = await factory.Service.CreateUserClaims(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to add claim.", result.Error);
        }

        [Fact]
        public async Task CreateUserClaims_WithValidRequest_ShouldCreateClaim()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(
                email: " user@example.com ",
                claimType: " Permission ",
                claimValue: " ManageElection ");

            User user = ClaimsTestData.CreateUser();

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(user);

            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user))
                .ReturnsAsync(new List<Claim>());

            factory.UserManager.Setup(userManager => userManager.AddClaimAsync(user,
                It.Is<Claim>(claim => claim.Type == "Permission" && claim.Value == "ManageElection")))
                .ReturnsAsync(IdentityResult.Success);

            Result<UserClaimsResponse> result = await factory.Service.CreateUserClaims(request);

            Assert.Equal(ResultStatus.Created, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal("Permission", result.Value.ClaimType);
            Assert.Equal("ManageElection", result.Value.ClaimValue);

            factory.UserManager.Verify(userManager => userManager.AddClaimAsync(user,
                It.Is<Claim>(claim => claim.Type == "Permission" && claim.Value == "ManageElection")), Times.Once);
        }

        [Fact]
        public async Task DeleteClaims_WithMissingUser_ShouldReturnNotFound()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(email: " user@example.com ");

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync((User?)null);

            Result<string> result = await factory.Service.DeleteClaims(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("User with email  user@example.com  was not found", result.Error);

            factory.UserManager.Verify(userManager => userManager.GetClaimsAsync(It.IsAny<User>()), Times.Never);
            factory.UserManager.Verify(userManager => userManager.RemoveClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()), Times.Never);
        }

        [Fact]
        public async Task DeleteClaims_WithMissingClaim_ShouldReturnNotFound()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest();
            User user = ClaimsTestData.CreateUser();

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());

            Result<string> result = await factory.Service.DeleteClaims(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("The claim was not found for this user", result.Error);

            factory.UserManager.Verify(userManager => userManager.RemoveClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()), Times.Never);
        }

        [Fact]
        public async Task DeleteClaims_WhenIdentityDeletionFails_ShouldReturnValidationError()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest();
            User user = ClaimsTestData.CreateUser();

            IList<Claim> claims = new List<Claim>
            {
                ClaimsTestData.CreateClaim()
            };

            IdentityError error = new()
            {
                Description = "Unable to remove claim."
            };

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user)).ReturnsAsync(claims);
            factory.UserManager.Setup(userManager => userManager.RemoveClaimAsync(user, It.Is<Claim>(claim => claim.Type == "Permission" && claim.Value == "ManageElection"))).ReturnsAsync(IdentityResult.Failed(error));

            Result<string> result = await factory.Service.DeleteClaims(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to remove claim.", result.Error);
        }

        [Fact]
        public async Task DeleteClaims_WithExistingClaim_ShouldDeleteClaim()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(email: " user@example.com ", claimType: " Permission ", claimValue: " ManageElection ");
            User user = ClaimsTestData.CreateUser();

            IList<Claim> claims = new List<Claim>
            {
                ClaimsTestData.CreateClaim()
            };

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user)).ReturnsAsync(claims);
            factory.UserManager.Setup(userManager => userManager.RemoveClaimAsync(user, It.Is<Claim>(claim => claim.Type == "Permission" && claim.Value == "ManageElection"))).ReturnsAsync(IdentityResult.Success);

            Result<string> result = await factory.Service.DeleteClaims(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.Equal("User removed from claim successfully", result.Value);

            factory.UserManager.Verify(userManager => userManager.RemoveClaimAsync(user, It.Is<Claim>(claim => claim.Type == "Permission" && claim.Value == "ManageElection")), Times.Once);
        }

        [Fact]
        public async Task EditUserClaims_WithMissingUser_ShouldReturnNotFound()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(email: " user@example.com ", oldValue: "CreateElection");

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync((User?)null);

            Result<UserClaimsResponse> result = await factory.Service.EditUserClaims(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("User with email  user@example.com  was not found", result.Error);

            factory.UserManager.Verify(userManager => userManager.GetClaimsAsync(It.IsAny<User>()), Times.Never);
            factory.UserManager.Verify(userManager => userManager.ReplaceClaimAsync(It.IsAny<User>(), It.IsAny<Claim>(), It.IsAny<Claim>()), Times.Never);
        }

        [Fact]
        public async Task EditUserClaims_WithEmptyOldValue_ShouldReturnValidationError()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(oldValue: " ");
            User user = ClaimsTestData.CreateUser();

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);

            Result<UserClaimsResponse> result = await factory.Service.EditUserClaims(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Old claim value cannot be empty.", result.Error);

            factory.UserManager.Verify(userManager => userManager.GetClaimsAsync(It.IsAny<User>()), Times.Never);
            factory.UserManager.Verify(userManager => userManager.ReplaceClaimAsync(It.IsAny<User>(), It.IsAny<Claim>(), It.IsAny<Claim>()), Times.Never);
        }

        [Fact]
        public async Task EditUserClaims_WithMissingOldClaim_ShouldReturnNotFound()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(claimValue: "ManageElection", oldValue: "CreateElection");
            User user = ClaimsTestData.CreateUser();

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());

            Result<UserClaimsResponse> result = await factory.Service.EditUserClaims(request);

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("The claim to edit was not found for this user", result.Error);

            factory.UserManager.Verify(userManager => userManager.ReplaceClaimAsync(It.IsAny<User>(), It.IsAny<Claim>(), It.IsAny<Claim>()), Times.Never);
        }

        [Fact]
        public async Task EditUserClaims_WithExistingNewClaim_ShouldReturnConflict()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(claimValue: "ManageElection", oldValue: "CreateElection");
            User user = ClaimsTestData.CreateUser();

            IList<Claim> claims = new List<Claim>
            {
                ClaimsTestData.CreateClaim("Permission", "CreateElection"),
                ClaimsTestData.CreateClaim("Permission", "ManageElection")
            };

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user)).ReturnsAsync(claims);

            Result<UserClaimsResponse> result = await factory.Service.EditUserClaims(request);

            Assert.Equal(ResultStatus.Conflict, result.Status);
            Assert.Equal("The user already has the new claim", result.Error);

            factory.UserManager.Verify(userManager => userManager.ReplaceClaimAsync(It.IsAny<User>(), It.IsAny<Claim>(), 
                It.IsAny<Claim>()), Times.Never);
        }

        [Fact]
        public async Task EditUserClaims_WhenIdentityUpdateFails_ShouldReturnValidationError()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(claimValue: "ManageElection", oldValue: "CreateElection");
            User user = ClaimsTestData.CreateUser();

            IList<Claim> claims = new List<Claim>
            {
                ClaimsTestData.CreateClaim("Permission", "CreateElection")
            };

            IdentityError error = new()
            {
                Description = "Unable to update claim."
            };

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user)).ReturnsAsync(claims);
            factory.UserManager.Setup(userManager => userManager.ReplaceClaimAsync(user, It.Is<Claim>(claim => claim.Type == "Permission" 
                && claim.Value == "CreateElection"), It.Is<Claim>(claim => claim.Type == "Permission" && claim.Value == "ManageElection"))).ReturnsAsync(IdentityResult.Failed(error));

            Result<UserClaimsResponse> result = await factory.Service.EditUserClaims(request);

            Assert.Equal(ResultStatus.ValidationError, result.Status);
            Assert.Equal("Unable to update claim.", result.Error);
        }

        [Fact]
        public async Task EditUserClaims_WithValidRequest_ShouldUpdateClaim()
        {
            ClaimsServiceFactory factory = new();

            UserClaimsRequest request = ClaimsTestData.CreateRequest(email: " user@example.com ", claimType: " Permission ", claimValue: " ManageElection ", oldValue: " CreateElection ");
            User user = ClaimsTestData.CreateUser();

            IList<Claim> claims = new List<Claim>
            {
                ClaimsTestData.CreateClaim("Permission", "CreateElection")
            };

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user)).ReturnsAsync(claims);
            factory.UserManager.Setup(userManager => userManager.ReplaceClaimAsync(user, It.Is<Claim>(claim => claim.Type == "Permission" 
                && claim.Value == "CreateElection"), It.Is<Claim>(claim => claim.Type == "Permission" && claim.Value == "ManageElection"))).ReturnsAsync(IdentityResult.Success);

            Result<UserClaimsResponse> result = await factory.Service.EditUserClaims(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal("user@example.com", result.Value.Email);
            Assert.Equal("Permission", result.Value.ClaimType);
            Assert.Equal("ManageElection", result.Value.ClaimValue);
            Assert.Equal("CreateElection", result.Value.OldValue);

            factory.UserManager.Verify(userManager => userManager.ReplaceClaimAsync(user, It.Is<Claim>(claim => claim.Type == "Permission" 
                && claim.Value == "CreateElection"), It.Is<Claim>(claim => claim.Type == "Permission" && claim.Value == "ManageElection")), Times.Once);
        }

        [Fact]
        public async Task GetUserClaims_WithMissingUser_ShouldReturnNotFound()
        {
            ClaimsServiceFactory factory = new();

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync((User?)null);

            Result<IEnumerable<UserClaimsResponse>> result = await factory.Service.GetUserClaims("user@example.com");

            Assert.Equal(ResultStatus.NotFound, result.Status);
            Assert.Equal("User with email user@example.com was not found", result.Error);

            factory.UserManager.Verify(userManager => userManager.GetClaimsAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetUserClaims_WithExistingUser_ShouldReturnClaims()
        {
            ClaimsServiceFactory factory = new();

            User user = ClaimsTestData.CreateUser();

            IList<Claim> claims = new List<Claim>
            {
                ClaimsTestData.CreateClaim("Permission", "ManageElection"),
                ClaimsTestData.CreateClaim("Permission", "ViewElection")
            };

            factory.UserManager.Setup(userManager => userManager.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            factory.UserManager.Setup(userManager => userManager.GetClaimsAsync(user)).ReturnsAsync(claims);

            Result<IEnumerable<UserClaimsResponse>> result = await factory.Service.GetUserClaims(" user@example.com ");

            Assert.Equal(ResultStatus.Success, result.Status);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count());
            Assert.Contains(result.Value, claim => claim.ClaimType == "Permission" && claim.ClaimValue == "ManageElection");
            Assert.Contains(result.Value, claim => claim.ClaimType == "Permission" && claim.ClaimValue == "ViewElection");

            factory.UserManager.Verify(userManager => userManager.GetClaimsAsync(user), Times.Once);
        }

        [Fact]
        public async Task GetRouteNames_WithSuccessfulSwaggerResponse_ShouldReturnOperationIds()
        {
            string responseContent = """
            {
                "paths": {
                    "/api/test": {
                        "get": {
                            "operationId": "GetTest"
                        },
                        "post": {
                            "operationId": "CreateTest"
                        }
                    }
                }
            }
            """;

            TestHttpMessageHandler messageHandler = new(HttpStatusCode.OK, responseContent);

            HttpClient httpClient = new(messageHandler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            ClaimsServiceFactory factory = new();

            factory.HttpClientFactory.Setup(httpClientFactory => httpClientFactory.CreateClient(nameof(ClaimsService)))
                .Returns(httpClient);

            List<string> result = await factory.Service.GetRouteNames("https://localhost");

            Assert.Equal(2, result.Count);
            Assert.Contains("GetTest", result);
            Assert.Contains("CreateTest", result);

            factory.HttpClientFactory.Verify(httpClientFactory => httpClientFactory.CreateClient(nameof(ClaimsService)), Times.Once);
        }

        [Fact]
        public async Task GetRouteNames_WhenSwaggerReturnsFailure_ShouldReturnEmptyList()
        {
            TestHttpMessageHandler messageHandler = new(HttpStatusCode.InternalServerError, string.Empty);

            HttpClient httpClient = new(messageHandler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            ClaimsServiceFactory factory = new();

            factory.HttpClientFactory
                .Setup(httpClientFactory => httpClientFactory.CreateClient(nameof(ClaimsService)))
                .Returns(httpClient);

            List<string> result = await factory.Service.GetRouteNames("https://localhost");

            Assert.Empty(result);

            factory.LoggerMessage.Verify(logger => logger.LogWarn("Route names request failed because the Swagger document could not be retrieved."),
                Times.Once);
        }

        private sealed class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _responseContent;

            public TestHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
            {
                _statusCode = statusCode;
                _responseContent = responseContent;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                HttpResponseMessage response = new(_statusCode)
                {
                    Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
                };

                return Task.FromResult(response);
            }
        }
    }
}