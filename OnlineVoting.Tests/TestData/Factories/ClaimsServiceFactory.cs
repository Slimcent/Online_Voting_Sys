using Microsoft.AspNetCore.Identity;
using Moq;
using OnlineVoting.Models.Entities;
using OnlineVoting.Services.Implementation;
using OnlineVoting.Services.Interfaces;
using VotingSystem.Logger;

namespace OnlineVoting.Tests.TestData.Factories
{
    public class ClaimsServiceFactory
    {
        public Mock<IUserStore<User>> UserStore { get; }
        public Mock<UserManager<User>> UserManager { get; }
        public Mock<IServiceFactory> ServiceFactory { get; }
        public Mock<ILoggerMessage> LoggerMessage { get; }
        public ClaimsService Service { get; }

        public ClaimsServiceFactory()
        {
            UserStore = new Mock<IUserStore<User>>();

            UserManager = new Mock<UserManager<User>>(
                UserStore.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            ServiceFactory = new Mock<IServiceFactory>();
            LoggerMessage = new Mock<ILoggerMessage>();

            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<UserManager<User>>()).Returns(UserManager.Object);
            ServiceFactory.Setup(serviceFactory => serviceFactory.GetService<ILoggerMessage>()).Returns(LoggerMessage.Object);

            Service = new ClaimsService(ServiceFactory.Object);
        }
    }
}