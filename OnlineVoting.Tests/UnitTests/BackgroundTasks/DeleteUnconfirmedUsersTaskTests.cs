using Microsoft.Extensions.Configuration;
using Moq;
using OnlineVoting.Services.BackgroundTasks;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Tests.UnitTests.BackgroundTasks
{
    public class DeleteUnconfirmedUsersTaskTests
    {
        [Fact]
        public async Task Execute_WithConfiguredRetentionDays_ShouldDeleteUnconfirmedUsers()
        {
            Dictionary<string, string?> configurationValues = new()
            {
                ["BackgroundJobs:UnconfirmedUserRetentionDays"] = "7"
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

            Mock<IUserService> userService = new();

            userService.Setup(service => service.DeleteUnconfirmedUsers(7))
                .ReturnsAsync(1);

            DeleteUnconfirmedUsersTask task = new(userService.Object, configuration);

            await task.Execute();

            userService.Verify(service => service.DeleteUnconfirmedUsers(7), Times.Once);
        }

        [Fact]
        public async Task Execute_WithMissingRetentionDays_ShouldUseDefaultRetentionDays()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .Build();

            Mock<IUserService> userService = new();

            userService.Setup(service => service.DeleteUnconfirmedUsers(5))
                .ReturnsAsync(1);

            DeleteUnconfirmedUsersTask task = new(userService.Object, configuration);

            await task.Execute();

            userService.Verify(service => service.DeleteUnconfirmedUsers(5), Times.Once);
        }
    }
}